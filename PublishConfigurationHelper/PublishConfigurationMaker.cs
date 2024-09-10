using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Paradox_Mods_Upload_Helper.Systems;

namespace Paradox_Mods_Upload_Helper.PublishConfigurationHelper
{
    internal static class PublishConfigurationMaker
    {
        // Method to retrieve values from UISystem
        private static string GetDisplayName()
        {
            // Example method to get the display name from UISystem
            // Replace with actual UISystem call
            return UISystem.GetTitle();
        }

        private static string GetThumbnailFilePath()
        {
            // Example method to get the thumbnail file path from UISystem
            // Replace with actual UISystem call
            return UISystem.GetThumbnailFilePath();
        }

        // Method without default filePath value
        public static void CreatePublishConfiguration(
            string modId,
            string shortDescription,
            string longDescription,
            string thumbnail,
            List<string> screenshots,
            List<string> tags,
            string forumLink,
            string modVersion,
            string gameVersion,
            List<string> dependencies,
            string changeLog,
            List<(string Type, string Url)> externalLinks,
            string filePath)
        {
            // Retrieve values from UISystem if necessary
            string displayName = GetDisplayName();
            // Use the provided thumbnail value
            // string thumbnail = GetThumbnailFilePath(); // This line is not needed if thumbnail is provided as a parameter

            // Create the root element
            XElement publishElement = new XElement("Publish",
                new XElement("ModId", new XAttribute("Value", modId)),
                new XElement("DisplayName", new XAttribute("Value", displayName)),
                new XElement("ShortDescription", new XAttribute("Value", shortDescription)),
                new XElement("LongDescription", longDescription),
                new XElement("Thumbnail", new XAttribute("Value", thumbnail)) // Ensure thumbnail is included
            );

            // Add screenshots if any
            foreach (var screenshot in screenshots)
            {
                publishElement.Add(new XElement("Screenshot", new XAttribute("Value", screenshot)));
            }

            // Add tags if any
            foreach (var tag in tags)
            {
                publishElement.Add(new XElement("Tag", new XAttribute("Value", tag)));
            }

            // Add forum link
            publishElement.Add(new XElement("ForumLink", new XAttribute("Value", forumLink)));

            // Add mod version
            publishElement.Add(new XElement("ModVersion", new XAttribute("Value", modVersion)));

            // Add game version
            publishElement.Add(new XElement("GameVersion", new XAttribute("Value", gameVersion)));

            // Add dependencies if any
            foreach (var dependency in dependencies)
            {
                publishElement.Add(new XElement("Dependency", new XAttribute("Id", dependency)));
            }

            // Add change log
            publishElement.Add(new XElement("ChangeLog", changeLog));

            // Add external links if any
            foreach (var externalLink in externalLinks)
            {
                publishElement.Add(new XElement("ExternalLink",
                    new XAttribute("Type", externalLink.Type),
                    new XAttribute("Url", externalLink.Url)));
            }

            // Save the XML to the specified file path
            XDocument xmlDocument = new XDocument(publishElement);
            xmlDocument.Save(filePath);
        }

        // Overloaded method with default filePath
        public static void CreatePublishConfiguration(
            string modId,
            string shortDescription,
            string longDescription,
            string thumbnail,
            List<string> screenshots,
            List<string> tags,
            string forumLink,
            string modVersion,
            string gameVersion,
            List<string> dependencies,
            string changeLog,
            List<(string Type, string Url)> externalLinks)
        {
            // Call the method with a default file path
            CreatePublishConfiguration(
                modId,
                shortDescription,
                longDescription,
                thumbnail,
                screenshots,
                tags,
                forumLink,
                modVersion,
                gameVersion,
                dependencies,
                changeLog,
                externalLinks,
                Path.Combine(GlobalPaths.AssemblyDirectory, "PublishConfiguration.xml")); // Provide a default file path
        }
    }
}
