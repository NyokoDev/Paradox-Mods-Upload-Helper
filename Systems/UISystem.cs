using Colossal.UI.Binding;
using Game.UI;
using Paradox_Mods_Upload_Helper.PublishConfigurationHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

namespace Paradox_Mods_Upload_Helper.Systems
{
    internal partial class UISystem : UISystemBase
    {

        private static string ThumbnailFilePath;


        // 
        public string UploadStatus { get; set; }
        public static string TemporaryTitle;
        public static string TemporaryDescription;
        public static string TemporaryIcon;

        // Class-level variable to store the selected file paths if needed
        private string[] SelectedFilesArray;



        protected override void OnCreate()
        {
            base.OnCreate();
            ClearExistingDirectory();
            ThumbnailFilePath = GetThumbnail();
            AddUpdateBinding(new GetterValueBinding<string>(Mod.MODUI, "Thumbnail", GetThumbnail));
            AddBinding(new TriggerBinding<string>(Mod.MODUI, "SendTitle", SendTitle));
            AddBinding(new TriggerBinding(Mod.MODUI, "SendThumbnail", SendThumbnail));
            AddBinding(new TriggerBinding(Mod.MODUI, "HandleSelectItemsClick", HandleSelectItemsClick));
            AddBinding(new TriggerBinding(Mod.MODUI, "Upload", Upload));
            AddUpdateBinding(new GetterValueBinding<string>(Mod.MODUI, "UploadStatus", GetUploadStatus));

        }

        private void ClearExistingDirectory()
        {
            if (GlobalPaths.AssemblyDirectory != null && Directory.Exists(GlobalPaths.AssemblyDirectory))
            {
                Directory.Delete(GlobalPaths.AssemblyDirectory, true); // 'true' to recursively delete all subdirectories and files
            }
        }


        private string GetUploadStatus()
        {
            if (string.IsNullOrEmpty(UploadStatus))
            {
                UploadStatus = "Ready for Upload!";
            }
            return UploadStatus;
        }

        private void Upload()
        {
            UploadStatus = "Uploading....";
            EnsureDirectory();
            PublishConfigurationMaker.CreatePublishConfiguration(
                modId: TemporaryTitle,
                shortDescription: "My mod",
                longDescription: "Upload custom content to Paradox Mods using Paradox Mods Upload Assistant.",
                thumbnail: ThumbnailFilePath,
                screenshots: new List<string> { },
                tags: new List<string> { },
                forumLink: "",
                modVersion: "1.0",
                gameVersion: "1.1.*",
                dependencies: new List<string> { },
                changeLog: "Initial release.",
                externalLinks: new List<(string Type, string Url)>
                {
                    // Add actual external links here if needed
                    // Example:
                    // ("discord", "https://discord.gg/example"),
                    // ("github", "https://github.com/example")
                },
                filePath: Path.Combine(GlobalPaths.AssemblyDirectory, "PublishConfiguration.xml") // Custom file path

            );

            NextUploadStep();
        }

        private void NextUploadStep()
        {
            // Start by setting the status to indicate copying content files
            UploadStatus = "Copying content files........";
            Mod.log.Info(UploadStatus); // Log the current status

            // Copy the content files to the target directory
            try
            {
                CopyContentFiles();
                UploadStatus = "Content files copied successfully.";
                Mod.log.Info(UploadStatus); // Log the status after successful copy
            }
            catch (Exception ex)
            {
                UploadStatus = "Error copying content files.";
                Mod.log.Error($"Error copying content files: {ex.Message}");
                Mod.log.Info(UploadStatus); // Log the error status
                return;
            }

            // Define paths
            string modPublisherFilePath = @"C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II\Cities2_Data\StreamingAssets\~Tooling~\ModPublisher\ModPublisher.exe";
            string publishConfigurationFilePath = Path.Combine(GlobalPaths.AssemblyDirectory, "PublishConfiguration.xml");
            string contentFilePaths = Path.Combine(GlobalPaths.AssemblyDirectory, "Content");

            // Check if ModPublisher.exe exists
            if (!File.Exists(modPublisherFilePath))
            {
                UploadStatus = $"ModPublisher.exe not found at: {modPublisherFilePath}";
                Mod.log.Error(UploadStatus);
                Mod.log.Info(UploadStatus); // Log the status when ModPublisher.exe is not found
                return;
            }

            // Prepare arguments for the ModPublisher
            string arguments = $"Publish \"{publishConfigurationFilePath}\" -c \"{contentFilePaths}\" -v";

            // Start the ModPublisher process
            UploadStatus = "Starting ModPublisher...";
            Mod.log.Info(UploadStatus); // Log the status before starting ModPublisher

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = modPublisherFilePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = Process.Start(processStartInfo))
                {
                    // Read output and errors
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    // Log output and errors
                    Mod.log.Info("ModPublisher Output: " + output);
                    if (!string.IsNullOrEmpty(error))
                    {
                        UploadStatus = "Error during upload.";
                        Mod.log.Error("ModPublisher Error: " + error);
                    }
                    else
                    {
                        // Extract the mod ID from the output
                        string modId = ExtractModIdFromOutput(output);

                        // Concatenate with the URL
                        string modUrl = $"https://mods.paradoxplaza.com/mods/{modId}";

                        UploadStatus = $"Uploaded successfully! The mod published with Id = {modId}. View it at: {modUrl}";
                        Mod.log.Info(UploadStatus); // Log the status after ModPublisher completes

                        // Open the mod URL in the default web browser
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = modUrl,
                                UseShellExecute = true // This opens the URL in the default browser
                            });
                        }
                        catch (Exception ex)
                        {
                            Mod.log.Error($"Failed to open URL: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UploadStatus = "Failed to start ModPublisher.";
                Mod.log.Error($"Error starting ModPublisher: {ex.Message}");
                Mod.log.Info(UploadStatus); // Log the status if ModPublisher fails to start
            }
        }



        // Method to extract mod ID from the output
        private string ExtractModIdFromOutput(string output)
        {
            // Implement logic to extract mod ID from the output
            // This is a placeholder implementation
            var match = Regex.Match(output, @"The mod published with Id = (\d+)");
            return match.Success ? match.Groups[1].Value : "Unknown";
        }


        private void CopyContentFiles()
        {
            // Ensure the target directory exists
            string targetDirectory = Path.Combine(GlobalPaths.AssemblyDirectory, "Content");
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // Assume SelectedFilesArray is an array of file paths to copy
            // Example: string[] SelectedFilesArray = { "path/to/file1.png", "path/to/file2.png" };

            foreach (var sourceFilePath in SelectedFilesArray)
            {
                if (File.Exists(sourceFilePath))
                {
                    // Create a target file path
                    string fileName = Path.GetFileName(sourceFilePath);
                    string targetFilePath = Path.Combine(targetDirectory, fileName);

                    // Copy the file to the target directory
                    File.Copy(sourceFilePath, targetFilePath, overwrite: true);
                }
                else
                {
                    // Handle the case where the source file does not exist
                    Console.WriteLine($"File not found: {sourceFilePath}");
                }
            }
        }

        private void EnsureDirectory()
        {
            if (!Directory.Exists(GlobalPaths.AssemblyDirectory))
            {
                Directory.CreateDirectory(GlobalPaths.AssemblyDirectory);
            }
        }


        private void HandleSelectItemsClick()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Allow selection of multiple files
                openFileDialog.Multiselect = true;

                // Exclude .exe files by specifying the filter (any file types except .exe)
                openFileDialog.Filter = "All Files (*.*)|*.*|Images (*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = "Select Files";

                // Show the file dialog and check if the user selected files
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the selected file paths
                    string[] selectedFilePaths = openFileDialog.FileNames;

                    // Filter out any .exe files (just to be extra cautious)
                    var validFilePaths = selectedFilePaths.Where(file => !file.EndsWith(".exe")).ToArray();

                    // Perform any actions with the valid file paths array (e.g., send them somewhere)
                    foreach (var filePath in validFilePaths)
                    {
                        Mod.log.Info("Selected file: " + filePath);
                    }

                    // Optionally, you can store the array in a class-level variable for future use
                    SelectedFilesArray = validFilePaths;
                    Mod.log.Info(SelectedFilesArray);
                }
            }
        }




        protected override void OnUpdate()
        {
        }

        private void SendThumbnail()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Filter to allow only image file types
                openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";
                openFileDialog.Title = "Select a Thumbnail";
                openFileDialog.ShowHelp = false;

                // Show the file dialog and check if the user selected a file
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the selected file path
                    string selectedFilePath = openFileDialog.FileName;

                    // Update the ThumbnailFilePath with the selected file path
                    ThumbnailFilePath = selectedFilePath;

                    // Optionally log or handle the updated file path
                    Mod.log.Info("Thumbnail file path updated to: " + ThumbnailFilePath + " Title " + TemporaryTitle);
                }
            }
        }



        private void SendTitle(string obj)
        {
            TemporaryTitle = obj;
        }


        private string GetThumbnail()
        {
            // Define a default image URL
            const string defaultImageUrl = "https://i.imgur.com/42B3e4O.png";

            // Return the default image URL if ThumbnailFilePath is null, empty, or not a well-formed URL
            if (string.IsNullOrEmpty(ThumbnailFilePath) || !Uri.IsWellFormedUriString(ThumbnailFilePath, UriKind.Absolute))
            {
                return defaultImageUrl;
            }

            // Return ThumbnailFilePath if it is a valid URL
            return ThumbnailFilePath;
        }

        internal static string GetThumbnailFilePath()
        {
            return ThumbnailFilePath;
        }

        internal static string GetTitle()
        {
            return TemporaryTitle;
        }
    }
}