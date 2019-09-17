# Build

To build the extension you have two options.

## Option 1: Visual Studio

1. Download Visual Studio 2019 (https://visualstudio.microsoft.com/de/vs/). When installing, select *Visual Studio extension development* under *Workloads*. (Alternatively, you can now continue with step 2 of option 2)
2. Open the solution (`CodyzeVSPlugin.sln`) in Visual Studio.
3. Under *Build->Configuration Manager* switch *Active solution configuration* to *Release*, then press *Close*.
4. Select *Build->Build Solution*.

## Option 2: MSBuild (Recommended if you don't use VS and just want to build)

1. Download the Visual Studio Build Tools (https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=BuildTools&rel=16). When installing, select *Visual Studio extension development* under *Workloads*.
2. Open `cmd` and execute
    ```
    $INSTALL_DIR\MSBuild\Current\Bin\MSBuild.exe CodyzeVSPlugin.sln /p:Configuration=Release
    ```
   where `$INSTALL_DIR` needs to be replaced with the install location of Visual Studio or Visual Studio Build Tools (e.g. `C:\Program Files (x86)\Microsoft Visual Studio\2019\Build Tools`).

# Install

After building, the packaged extension can be found in `CodyzeVSPlugin\bin\Release\CodyzeVSPlugin.vsix`. To install, just double-click the file.
