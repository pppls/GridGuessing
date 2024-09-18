using Backend;
using Backend.Data;
using Common;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Test;

public class GridHubTest
{
    //Deze test gaat af als het mogelijk is dat dezelfde prijs twee keer
    //toegereikt kan worden aan iemand (doordat tegelijkertijd de db geaccessed wordt bijvoorbeeld)
    [Fact]
    public async void OnlyOneClientCanWinAGivenPrizeTest()
    {
        //Arrange
        var dbContext = TestHelper.Setup("Data Source=SharedTestDb;Mode=Memory;Cache=Shared");
        var dbContext2 = TestHelper.CreateTestDbContext("Data Source=SharedTestDb;Mode=Memory;Cache=Shared");
        
        var mockClients = new Mock<IHubCallerClients>();
        var mockCallerClient = new Mock<ISingleClientProxy>();
        var mockOthersClient = new Mock<ISingleClientProxy>();
        var mockHubCallerContext = new Mock<HubCallerContext>();
        var mockHubCallerContext2 = new Mock<HubCallerContext>();
        
        mockClients.Setup(clients => clients.Caller).Returns(mockCallerClient.Object);
        mockClients.Setup(clients => clients.Others).Returns(mockOthersClient.Object);
        mockHubCallerContext.Setup(Context => Context.ConnectionId).Returns("1");
        mockHubCallerContext2.Setup(Context => Context.ConnectionId).Returns("2");
        
        var hub1 = new GridHub(dbContext) { Clients = mockClients.Object, Context = mockHubCallerContext.Object};
        var hub2 = new GridHub(dbContext2) { Clients = mockClients.Object, Context = mockHubCallerContext2.Object };
        
        var gridElementsWithPrize = dbContext.GridElements
            .Include(gridElement => gridElement.Prize!).Where(element => element.Prize is MonetaryPrize);
        
        //Act
        var selectedPrize = gridElementsWithPrize.Include(gridElement => gridElement.Prize!).First();
        var flip1 = Task.Run(() => hub1.FlipGridElement(selectedPrize.Index));
        var flip2 = Task.Run(() => hub2.FlipGridElement(selectedPrize.Index));
        Task<GridElementExt>[] tasks = [flip1, flip2];
        var results = await Task.WhenAll(tasks);
        var prizeFirstTime = (FlippedGridElementExt)results[0];
        var prizeSecondTime = (FlippedGridElementExt)results[1];
        
        //Assert
        var firstMonetaryPrizeResult = (MonetaryPrizeResult)prizeFirstTime.result;
        var secondMonetaryPrizeResult = (MonetaryPrizeResult)prizeSecondTime.result;
        bool firstIsFirstFlipper = firstMonetaryPrizeResult.areYouTheFirstFlipper &&
                                   !secondMonetaryPrizeResult.areYouTheFirstFlipper;
        bool secondIsFirstFlipper = !firstMonetaryPrizeResult.areYouTheFirstFlipper &&
                                    secondMonetaryPrizeResult.areYouTheFirstFlipper;
        
        (firstIsFirstFlipper || secondIsFirstFlipper).Should()
            .BeTrue("one should be the first flipper and the other should not");
        firstMonetaryPrizeResult.value.Should().Be(secondMonetaryPrizeResult.value);
        firstMonetaryPrizeResult.value.Should().Be((int)((MonetaryPrize)selectedPrize.Prize!).MonetaryValue);
    }
    
    [Fact]
    public async void WhenClientFlipsPrizeOtherClientsShouldReceiveUpdate_InitialFlipperIsFirstFlippersOthersArent_Test()
    {
        //Arrange
        var dbContext = TestHelper.Setup("Mode=Memory;Cache=Shared");
        
        var mockClients = new Mock<IHubCallerClients>();
        var mockCallerClient = new Mock<ISingleClientProxy>();
        var mockOthersClient = new Mock<ISingleClientProxy>();
        var mockHubCallerContext = new Mock<HubCallerContext>();
        
        mockClients.Setup(clients => clients.Caller).Returns(mockCallerClient.Object);
        mockClients.Setup(clients => clients.Others).Returns(mockOthersClient.Object);
        mockHubCallerContext.Setup(Context => Context.ConnectionId).Returns("1");
        
        mockOthersClient.Setup(client => client.SendCoreAsync(
                It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Verifiable("Message should be sent to others");;
        mockClients.Setup(clients => clients.Caller).Returns(mockCallerClient.Object);
        mockClients.Setup(clients => clients.Others).Returns(mockOthersClient.Object);
        
        var hub = new GridHub(dbContext) { Clients = mockClients.Object, Context = mockHubCallerContext.Object };
        
        var gridElementsWithPrize = dbContext.GridElements
            .Include(gridElement => gridElement.Prize!).Where(element => element.Prize is MonetaryPrize);
        
        //Act
        var selectedPrize = gridElementsWithPrize.Include(gridElement => gridElement.Prize!).First();
        var flippedGridElement = (FlippedGridElementExt) await hub.FlipGridElement(selectedPrize.Index);
        
        //Assert
        var monetaryPrizeResult = (MonetaryPrizeResult)flippedGridElement.result;
        
        //Others should receive result with areYouTheFirstFlipper false
        var expectedResultForOthers = monetaryPrizeResult with {areYouTheFirstFlipper = false};
        object[] expectedElementForOthers = [flippedGridElement with { result = expectedResultForOthers }];
        mockOthersClient.Verify(
            client => client.SendCoreAsync("UpdateGridElement", expectedElementForOthers, It.IsAny<CancellationToken>()),
            Times.Once(), // Ensures the method was called exactly once
            "Others should receive exactly one message");
        
        //Clicker should receive result with areYouTheFirstFlipper true
        monetaryPrizeResult.areYouTheFirstFlipper.Should().BeTrue();
    }
    
    [Fact]
    public async void WhenClientFlipsNonPrizeOtherClientsShouldReceiveUpdate_Test()
    {
        //Arrange
        var dbContext = TestHelper.Setup("Mode=Memory;Cache=Shared");
        
        var mockClients = new Mock<IHubCallerClients>();
        var mockCallerClient = new Mock<ISingleClientProxy>();
        var mockOthersClient = new Mock<ISingleClientProxy>();
        var mockHubCallerContext = new Mock<HubCallerContext>();
        
        mockOthersClient.Setup(client => client.SendCoreAsync(
                It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Verifiable("Message should be sent to others");;
        mockClients.Setup(clients => clients.Caller).Returns(mockCallerClient.Object);
        mockClients.Setup(clients => clients.Others).Returns(mockOthersClient.Object);
        mockHubCallerContext.Setup(Context => Context.ConnectionId).Returns("1");
        
        var hub = new GridHub(dbContext) { Clients = mockClients.Object, Context = mockHubCallerContext.Object};
        
        var gridElementsWithPrize = dbContext.GridElements
            .Include(gridElement => gridElement.Prize!).Where(element => element.Prize == null);
        
        //Act
        var selectedPrize = gridElementsWithPrize.Include(gridElement => gridElement.Prize!).First();
        var flippedGridElement = (FlippedGridElementExt) await hub.FlipGridElement(selectedPrize.Index);
        
        //Assert
        object[] expectedElementForOthers = [flippedGridElement];
        mockOthersClient.Verify(
            client => client.SendCoreAsync("UpdateGridElement", expectedElementForOthers, It.IsAny<CancellationToken>()),
            Times.Once(), // Ensures the method was called exactly once
            "Others should receive exactly one message");
        
        //Clicker should receive result with areYouTheFirstFlipper true
        (flippedGridElement.result is NoPrizeResult).Should().BeTrue();
    }
    
    [Fact]
    public async void WhenClientFlipsTwiceSecondTimeShouldNotUpdateElement_Test()
    {
        //Arrange
        var dbContext = TestHelper.Setup("Mode=Memory;Cache=Shared");
        
        var mockClients = new Mock<IHubCallerClients>();
        var mockCallerClient = new Mock<ISingleClientProxy>();
        var mockOthersClient = new Mock<ISingleClientProxy>();
        var mockHubCallerContext = new Mock<HubCallerContext>();
        
        mockOthersClient.Setup(client => client.SendCoreAsync(
                It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Verifiable("Message should be sent to others");;
        mockClients.Setup(clients => clients.Caller).Returns(mockCallerClient.Object);
        mockClients.Setup(clients => clients.Others).Returns(mockOthersClient.Object);
        mockHubCallerContext.Setup(Context => Context.ConnectionId).Returns("1");
        
        var hub = new GridHub(dbContext) { Clients = mockClients.Object, Context = mockHubCallerContext.Object};
        
        var gridElementsWithPrize = dbContext.GridElements
            .Include(gridElement => gridElement.Prize!).Where(element => element.Prize == null);
        
        //Act
        var selectedPrize = gridElementsWithPrize.Include(gridElement => gridElement.Prize!).First();
        var flippedGridElement = (FlippedGridElementExt) await hub.FlipGridElement(selectedPrize.Index);
        var selectedPrize2 = gridElementsWithPrize.Include(gridElement => gridElement.Prize!).ToList()[1];
        var flippedGridElement2 = await hub.FlipGridElement(selectedPrize2.Index);
        //Assert
        object[] expectedElementForOthers = [flippedGridElement];
        mockOthersClient.Verify(
            client => client.SendCoreAsync("UpdateGridElement", expectedElementForOthers, It.IsAny<CancellationToken>()),
            Times.Once(), // Ensures the method was called exactly once
            "Others should receive exactly one message");
        
        //Clicker should receive result with areYouTheFirstFlipper true
        (flippedGridElement.result is NoPrizeResult).Should().BeTrue();
        (flippedGridElement2 is UnflippedGridElementExt).Should().BeTrue();
    }
    
    [Fact]
    public async void ReceiveGrid_ElementUnflipped_Test()
    {
        //Arrange
        var dbContext = TestHelper.CreateTestDbContext("Mode=Memory;Cache=Shared");
        var gridPromotionalGame = new GridPromotionalGame
        {
            Name = "NLO",
            GridElements = new List<GridElement>()
        };
        var monetaryPrize = new MonetaryPrize
        {
            MonetaryValue = 50,
        };
        
        dbContext.PromotionalGames.Add(gridPromotionalGame);
        dbContext.Prizes.Add(monetaryPrize);
        dbContext.SaveChanges();
        var gridElement = new GridElement
        {
            Id = null,
            Index = 0,
            HasBeenFlipped = false,
            GridPromotionalGameId = dbContext.PromotionalGames.Single().Id,
            PrizeId = dbContext.Prizes.Single().Id
        };
        dbContext.GridElements.Add(gridElement);
        dbContext.SaveChanges();
        
        var mockClients = new Mock<IHubCallerClients>();
        var mockCallerClient = new Mock<ISingleClientProxy>();
        
        mockCallerClient.Setup(client => client.SendCoreAsync(
                It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Verifiable("Message should be sent to caller");;
        mockClients.Setup(clients => clients.Caller).Returns(mockCallerClient.Object);
        
        var hub = new GridHub(dbContext) { Clients = mockClients.Object };
        
        //Act
        await hub.OnConnectedAsync();
        
        //Assert
        var unflippedGridElementExt = new UnflippedGridElementExt(0);
        mockCallerClient.Verify(
            client => client.SendCoreAsync("ReceiveGrid", It.Is<object[]>(p => ((GridElementExt[])p[0])[0] == unflippedGridElementExt), It.IsAny<CancellationToken>()),
            Times.Once(), // Ensures the method was called exactly once
            "Caller should receive exactly one message");
    }
    
    [Fact]
    public async void ReceiveGrid_ElementWithPrizeFlipped_Test()
    {
        //Arrange
        var dbContext = TestHelper.CreateTestDbContext("Mode=Memory;Cache=Shared");
        var gridPromotionalGame = new GridPromotionalGame
        {
            Name = "NLO",
            GridElements = new List<GridElement>()
        };
        var monetaryPrize = new MonetaryPrize
        {
            MonetaryValue = 50,
        };
        
        dbContext.PromotionalGames.Add(gridPromotionalGame);
        dbContext.Prizes.Add(monetaryPrize);
        dbContext.SaveChanges();
        var gridElement = new GridElement
        {
            Id = null,
            Index = 0,
            HasBeenFlipped = true,
            GridPromotionalGameId = dbContext.PromotionalGames.Single().Id,
            PrizeId = dbContext.Prizes.Single().Id
        };
        dbContext.GridElements.Add(gridElement);
        dbContext.SaveChanges();
        
        var mockClients = new Mock<IHubCallerClients>();
        var mockCallerClient = new Mock<ISingleClientProxy>();
        
        mockCallerClient.Setup(client => client.SendCoreAsync(
                It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Verifiable("Message should be sent to caller");;
        mockClients.Setup(clients => clients.Caller).Returns(mockCallerClient.Object);
        
        var hub = new GridHub(dbContext) { Clients = mockClients.Object };
        
        //Act
        await hub.OnConnectedAsync();
        
        //Assert
        var unflippedGridElementExt = new FlippedGridElementExt(new MonetaryPrizeResult(false, 50), 0);
        mockCallerClient.Verify(
            client => client.SendCoreAsync("ReceiveGrid", It.Is<object[]>(p => ((GridElementExt[])p[0])[0] == unflippedGridElementExt), It.IsAny<CancellationToken>()),
            Times.Once(), // Ensures the method was called exactly once
            "Caller should receive exactly one message");
    }
    
    [Fact]
    public async void ReceiveGrid_ElementWithoutPrizeFlipped_Test()
    {
        //Arrange
        var dbContext = TestHelper.CreateTestDbContext("Mode=Memory;Cache=Shared");
        var gridPromotionalGame = new GridPromotionalGame
        {
            Name = "NLO",
            GridElements = new List<GridElement>()
        };
        
        dbContext.PromotionalGames.Add(gridPromotionalGame);
        dbContext.SaveChanges();
        var gridElement = new GridElement
        {
            Id = null,
            Index = 0,
            HasBeenFlipped = true,
            GridPromotionalGameId = dbContext.PromotionalGames.Single().Id,
            PrizeId = null
        };
        dbContext.GridElements.Add(gridElement);
        dbContext.SaveChanges();
        
        var mockClients = new Mock<IHubCallerClients>();
        var mockCallerClient = new Mock<ISingleClientProxy>();
        
        mockCallerClient.Setup(client => client.SendCoreAsync(
                It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Verifiable("Message should be sent to caller");;
        mockClients.Setup(clients => clients.Caller).Returns(mockCallerClient.Object);
        
        var hub = new GridHub(dbContext) { Clients = mockClients.Object };
        
        //Act
        await hub.OnConnectedAsync();
        
        //Assert
        var unflippedGridElementExt = new FlippedGridElementExt(new NoPrizeResult(), 0);
        mockCallerClient.Verify(
            client => client.SendCoreAsync("ReceiveGrid", It.Is<object[]>(p => ((GridElementExt[])p[0])[0] == unflippedGridElementExt), It.IsAny<CancellationToken>()),
            Times.Once(), // Ensures the method was called exactly once
            "Caller should receive exactly one message");
    }
}