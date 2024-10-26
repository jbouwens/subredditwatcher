Caveats: only tested with Visual Studio on Windows. Have not tried running it in Docker.

Open the sln with Visual Studio and copy local.settings.example.json to local.settings.json.
- Set clientid and clientsecret

Run the application.

Known issues:
- Sometimes you may need to press enter to continue execution. Not *exactly* sure why, but if you see the Cycle statistic stop going up, that's probably why.
- Couldn't get the emojis to show up in the console so they just display as ??. Tried to install a font but no luck.
- Once my token stopped working even though `duration=permenant?` Had to delete my token.json and have a new one generated.
- UI is not colorful. May address this later.
- Wanted to add more colorful commentary like "Woah, PostA overtook PostB for first!!!".
- Test coverage could be improved. Just did a couple examples.

Inspired by https://github.com/galydev/CleanArchitectureConsoleApp and https://github.com/jasontaylordev/RapidConsole. Tried to demonstrate clean arcitecture but keep it all in one project. Wanted to try Spectre.

![UI Screenshot](https://github.com/jbouwens/subredditwatcher/blob/master/ui-screenshot.png?raw=true "UI Screenshot")