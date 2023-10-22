using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PdfTools.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    protected ViewModelBase()
    {
        ErrorMessages = new ObservableCollection<string>();
    }
    
    [ObservableProperty] 
    private ObservableCollection<string>? _errorMessages;
}