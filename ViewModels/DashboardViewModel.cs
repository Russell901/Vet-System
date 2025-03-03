using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using Vet_System.Models;
using Windows.UI;

namespace Vet_System.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {

        public ObservableCollection<StatItem> Stats { get; } = new();
        public ObservableCollection<AppointmentItem> Appointments { get; } = new();
        public ObservableCollection<ActivityItem> Activities { get; } = new();

        public DashboardViewModel()
        {
            InitializeStats();
            InitializeAppointments();
            InitializeActivities();
        }

        private void InitializeStats()
        {
            Stats.Add(new StatItem
            {
                Title = "Total Patients",
                Value = "1,284",
                Icon = "\uE77B",
                Background = new SolidColorBrush(Color.FromArgb(255, 236, 253, 245)),
                IconColor = new SolidColorBrush(Color.FromArgb(255, 16, 185, 129))
            });

            Stats.Add(new StatItem
            {
                Title = "Appointments Today",
                Value = "12",
                Icon = "\uE787",
                Background = new SolidColorBrush(Color.FromArgb(255, 239, 246, 255)),
                IconColor = new SolidColorBrush(Color.FromArgb(255, 59, 130, 246))
            });

            // Add more stats as needed
        }

        private void InitializeAppointments()
        {
            Appointments.Add(new AppointmentItem
            {
                PatientName = "Max Smith",
                Description = "Annual Checkup",
                Time = "14:30"
            });

            // Add more appointments
        }

        private void InitializeActivities()
        {
            Activities.Add(new ActivityItem
            {
                Text = "New patient registration: Luna",
                Time = "10 minutes ago"
            });

            // Add more activities
        }

        public async void OnAppointmentClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is AppointmentItem appointment)
            {
                var dialog = new ContentDialog
                {
                    Title = "Appointment Details",
                    Content = $"Patient: {appointment.PatientName}\nDescription: {appointment.Description}\nTime: {appointment.Time}",
                    CloseButtonText = "OK"
                };

                if (sender is FrameworkElement element)
                {
                    dialog.XamlRoot = element.XamlRoot;
                    await dialog.ShowAsync();
                }
            }
        }

    }

}