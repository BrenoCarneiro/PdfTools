using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace PdfTools.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string? _filePath;
    [ObservableProperty] private string? _savePath;
    [ObservableProperty] private string? _textMessage;

    [RelayCommand]
    private async Task OpenFile(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            var file = await DoOpenFilePickerAsync();
            if (file is null) return;

            FilePath = file.TryGetLocalPath();

        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }
    
    [RelayCommand]
    private async Task SaveTo(CancellationToken token)
    {
        ErrorMessages?.Clear();
        try
        {
            var file = await DoSaveFilePickerAsync();
            if (file is null) return;

            SavePath = file.TryGetLocalPath();

        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
        }
    }

    [RelayCommand]
    private async Task SaveFile()
    {
        ErrorMessages?.Clear();
        try
        {
            if (SavePath is null) return;
            
            using (PdfReader pdfReader = new PdfReader(FilePath))
            {
                // Create a StreamWriter to write the extracted text to a file
                using (StreamWriter writer = new StreamWriter(SavePath))
                {
                    // Create a PdfDocument instance
                    using (PdfDocument pdfDoc = new PdfDocument(pdfReader))
                    {
                        // Iterate through each page and extract text
                        int numPages = pdfDoc.GetNumberOfPages();
                        for (int pageNum = 1; pageNum <= numPages; pageNum++)
                        {
                            string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(pageNum));
                            writer.Write(pageText);
                        }
                        TextMessage = "Success!";
                    }
                }
            }
        }
        catch (Exception e)
        {
            ErrorMessages?.Add(e.Message);
            TextMessage = "Error";
        }
    }

    private async Task<IStorageFile?> DoOpenFilePickerAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open PDF File",
            AllowMultiple = false,
            FileTypeFilter = new[] {FilePickerFileTypes.Pdf}
        });

        return files?.Count >= 1 ? files[0] : null;
    }

    private async Task<IStorageFile?> DoSaveFilePickerAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
            throw new NullReferenceException("Missing StorageProvider instance.");

        return await provider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save Text File",
            FileTypeChoices = new []{FilePickerFileTypes.TextPlain},
        });
    }
}