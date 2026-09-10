using System;
using System.IO;
using System.Threading.Tasks;

namespace Vayu.CommonAccessLibrary
{
    public class FileDeleter
    {
        /// <summary>
        /// Get
        /// </summary>
        /// <param name="directoryPath"></param>
        public async Task DeleteFilesAsync(string directoryPath)
        {
            try
            {
                // Get all file paths in the specified directory
                string[] filePaths = Directory.GetFiles(directoryPath);

                // Iterate through each file and delete it
                foreach (string filePath in filePaths)
                {
                    File.Delete(filePath);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle unauthorized access exceptions
                Console.WriteLine("Unauthorized access to file.");
                // Log or handle the error appropriately
            }
            catch (DirectoryNotFoundException)
            {
                // Handle directory not found exceptions
                Console.WriteLine("Directory not found.");
                // Log or handle the error appropriately
            }
            catch (IOException)
            {
                // Handle IO exceptions
                Console.WriteLine("An I/O error occurred while deleting files.");
                // Log or handle the error appropriately
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine("An unexpected error occurred: " + ex.Message);
                // Log or handle the error appropriately
            }
            finally
            {
                // Optionally, perform cleanup or finalization tasks here
                await Task.Delay(10000); // Pause execution for 10 seconds
            }
        }
    }
}