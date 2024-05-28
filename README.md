# Welcome to the Checkmate with ChatGPT workshop

Here you can find the needed files for this workshop. This repository consists out of two folders, 
*o* and *1*. The first folder is needed for the first part of the automation chapter of the workshop and is a "minimal" setup to get you started. The *1* folder contains the complete solution.
Each folder has it's own project and has all needed requirements satisfied. To run each project open a terminal (or through your own IDE) and type to generate the runtimes:
> dotnet build

after which you will need the Playwright runtime:
> pwsh bin/Debug/net8.0/playwright.ps1 install

After this is done you can run the project by typing from the project folder:
> dotnet run

You are now ready to start the workshop! Have fun!

## Useful links:
https://playwright.dev/dotnet/docs/intro
https://playwright.dev/dotnet/docs/input
https://playwright.dev/dotnet/docs/locators