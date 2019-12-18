using Caliburn.Micro;
using Regi.Models;

namespace Regi.UI.ViewModels
{
    public class HomeViewModel : Screen
    {
		public HomeViewModel()
		{
			Projects.Add(new Project { Name = "Backend" });
			Projects.Add(new Project { Name = "Frontend" });
		}

		private string _firstName;
		public string FirstName
		{
			get => _firstName;
			set
			{
				_firstName = value;
				NotifyOfPropertyChange(() => FirstName);
				NotifyOfPropertyChange(() => FullName);
			}
		}

		private string _lastName;

		public string LastName
		{
			get => _lastName;
			set
			{
				_lastName = value;
				NotifyOfPropertyChange(() => LastName);
				NotifyOfPropertyChange(() => FullName);
			}
		}

		public string FullName => $"{FirstName} {LastName}";

		private BindableCollection<Project> projects = new BindableCollection<Project>();

		public BindableCollection<Project> Projects
		{
			get => projects;
			set
			{
				projects = value;
			}
		}

		private Project _selectedProject;

		public Project SelectedProject
		{
			get => _selectedProject;
			set
			{
				_selectedProject = value;
				NotifyOfPropertyChange(() => SelectedProject);
			}
		}

		public bool CanClearText(string firstName, string lastName) => !string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName);

		public void ClearText(string firstName, string lastName)
		{
			FirstName = "";
			LastName = "";
		}
	}
}