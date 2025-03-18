# BadBuilder
BadBuilder is a tool for creating a BadUpdate USB drive for the Xbox 360. It automates the process of formatting the USB drive, downloading required files, extracting them, and allowing the addition of homebrew applications.

## Features
### USB Formatting (Windows Only)
- Uses a custom FAT32 formatter that supports large USB drives (≥32GB).
- Ensures compatibility with the Xbox 360.

> [!NOTE]  
> Currently, the formatting feature is **Windows-only**. If you compile BadBuilder for another OS, it'll prompt you to manually format your target disk.

### Automatic File Downloading
- Detects and downloads the latest required files automatically.
- Recognizes previously downloaded files and reuses them by default.
- Allows specifying custom paths for required files if they are already on your system.
> [!IMPORTANT]  
> BadBuilder does not dynamically locate files inside ZIP archives. If your provided archive has a different folder structure than expected, the process will fail abruptly. Ensure your archive matches the expected format if specifying an existing copy.

### File Extraction & Copying
- Extracts all necessary files automatically.
- Prepares the USB drive for the BadUpdate exploit by copying all required files.
### Homebrew Support
- Allows adding homebrew applications by specifying their root folder.
- Automatically searches for the entry point (`.xex`) file within the folder.
- If multiple `.xex` files are found, BadBuilder will prompt you to select the correct one.
- Copies all necessary files and patches the entry `.xex` using the downloaded XexTool.

## How to Use
1. **Launch the executable**. It will open inside of a Terminal window.
2. **Formatting (Windows Only):** BadBuilder will format your USB drive as FAT32, even if it’s larger than 32GB.
> [!CAUTION]
> Formatting a disk means that all data will be lost. Make sure you have selected the right drive before confirming the format. I am not responsible for any data loss.
4. **Download Files:** BadBuilder will fetch the required exploit files or let you specify an existing location.
5. **Extract Files:** BadBuilder will automatically extract everything needed.
6. **Copy files:** BadBuilder will copy all of the extracted files to the correct locations.
5. **Add Homebrew (Optional):**
    - Specify the root folder of your homebrew application (e.g., `D:\Aurora 0.7b.2 - Release Package`).
    - BadBuilder will locate the `.xex` file inside.
    - If multiple `.xex` files exist, you’ll be prompted to choose the correct entry point.
    - First, all necessary files will be copied, then, the `.xex` file will be patched using **XexTool**.
        - This ensures that the original copy of the homebrew program will **not** be modified, as it is instead done in-place on the USB drive.

## Example Homebrew Folder Structure
If you want to add Aurora, you would select the **root folder**, like:

```
D:\Aurora 0.7b.2 - Release Package
```

Which contains:

```
Aurora 0.7b.2 - Release Package/
├── Data/
├── Media/
├── Plugins/
├── Skins/
├── User/
├── Aurora.xex
├── live.json
├── nxeart
```
BadBuilder will detect `Aurora.xex` as the entry point and patch it accordingly.

> [!IMPORTANT]  
> Homebrew apps which do not contain the entry point in the root folder are not currently supported.

### Using unsupported homebrew
If you would still like to use unsupported homebrew apps, you'll have to copy them manually. To do so, perform these steps:
1. Copy the entire root folder into the `Apps` folder on your target drive.
2. Find and copy the path of the entry point.
3. Open a terminal window and construct the following command:
    - **Command:** `"<path_to_folder_of_BadBuilder.exe>\Work\Extract\BadUpdate Tools\XePatcher\XexTool.exe" -m r -r a "<path_to_entry_point>"`
    - **Example:** `"C:\BadBuilder\Work\Extract\BadUpdate Tools\XePatcher\XexTool.exe" -m r -r a "H:\Apps\Aurora 0.7b.2 - Release Package\Aurora.xex"`

## Reporting Issues
If you encounter any problems, please create a new issue with details about your setup and the problem.
