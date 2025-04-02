# Veterinary Appointment and Pet Management System

![WinUI3](https://img.shields.io/badge/WinUI3-3.0-blue) ![MySQL](https://img.shields.io/badge/MySQL-8.0-orange) ![MVVM](https://img.shields.io/badge/Pattern-MVVM-purple)

A modern desktop application for veterinary clinics to manage appointments, patient records, and pet health information, built with WinUI3 and MySQL.

## Features

### 🔐 Authentication System
- Basic username/password authentication
- Account lockout after 5 failed attempts
- Session management

### 🛠️ Automated Database Setup
- Self-initializing database on first launch
- Pre-seeded with default patient data
- Automatic table creation/migrations

### 🐾 Core Functionality
- Patient registration and management
- Appointment scheduling with reminders
- Medical history tracking (vaccinations, treatments)
- Client/pet relationship management
- Advanced search across all records

## ⚙️ Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download)
- [WinUI3](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [MySQL Server 8.0+](https://dev.mysql.com/downloads/mysql/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (Recommended)

## 🚀 Installation & Setup

1. **Clone Repository**
	   ```bash
	   git clone https://github.com/yourusername/vet-management-system.git
	   cd vet-management-system```

2. **Database Setup**
   - Install MySQL Community Server
   - Create a MySQL user with full privileges
   - Update connection string in appsettings.json:
	```json
		"ConnectionStrings": {
	  "DefaultConnection": "server=localhost;user=your_user;password=your_password;"
	}```

3. **Build and run the application**

🖥️ **Usage**
	1. First Launch
		- Database tables will auto-create
		- Default patient "Fluffy" added

	2. Authentication
		- Launch application
		- Enter credentials (create new account if needed)
		- After 5 failed attempts, account locks for 15 minutes

	3. Key Operations
		- Add Patient: Navigate to Patients → New
		- Schedule Appointment: Calendar view → Select date/time
		- Medical Records: Patient profile → Medical History
		- Search: Global search bar (name, phone, pet ID)

🛠️ **Tech Stack**
	
	- Frontend: WinUI3, XAML
	- Backend: C# (.NET 6)
	- Database: MySQL
	- Architecture: MVVM with Dependency Injection
	- Tools: Entity Framework Core, CommunityToolkit.MVVM

🤝 **Contributing**
	Contributions welcome! Please follow these steps:
		
		1. Fork the repository
		2. Create your feature branch (git checkout -b feature/AmazingFeature)
		3. Commit changes (git commit -m 'Add some AmazingFeature')
		4. Push to branch (git push origin feature/AmazingFeature)
		5. Open a Pull Request

📄 **License**
	Distributed under the MIT License. See LICENSE for more information.
		
**Note:** This system is designed for educational/demonstration purposes. Always implement additional security measures before production use.