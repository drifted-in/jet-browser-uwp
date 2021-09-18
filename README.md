# JetBrowser (UWP)
A desktop app for browsing parish book scans distributed in ZIP files.

## Building
1. Install Visual Studio 2019
2. Open the `jet-browser-uwp` project
3. In the configuration toolbar set the `Release` mode with `x64` architecture
4. Click `Local Machine` to build the solution locally
5. Verify the output stored in `jetbrowser\bin\x64\Release\jetbrowser.exe`

## Running
1. Download sample parish registers: [150-02792.zip](http://88.146.158.154:8083/150-02792.zip) (110 MB)
2. Locate downloaded file in the File Explorer, right click the zip file and in the Open with... submenu select the `jetbrowser` menu item

## Usage
- Use `<-`/`->` arrows keys to move to the previous/next image
- Use `Shift`/`Ctrl` modifiers together with arrow keys to increase the step to 5/20  
- Use `Home`/`End` keys to move to the first/last image
- Use `H`/`W` keys to scale the image to fit the height/width of the window
- Use `R` key to reset the size to the original image size
- Use `C` key to copy the current image filename to the clipboard