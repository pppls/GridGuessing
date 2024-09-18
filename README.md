GridGuessing

Simple concurrent guessing game that involves 100x100 squares that can be clicked to guess at prizes. The main prize is shown in blue upon reveal, consolation prizes are shown in green, and no prizes are shown in red.

To run: First run the backend project, once complete, run the frontend project.

* Backend written in ASP.NET Core, Frontend in Blazor WASM. SignalR used for real-time updates.

* Simultaneous users can be simulated via the AdditionalClientsController. The parameter dictates how many users will be clicking at the same time (once every second). It is recommended to keep n < 200 because blazor rendering starts to struggle (the backend has no issue at all).

* To prevent the same prize from being given to two different users due to both clicking the same grid element at the same time, I made use of optimistic concurrency.

* A user can only click once - if you want to click again, reload the page.

