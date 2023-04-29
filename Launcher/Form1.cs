using Swordie;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SwordieLauncher
{
    public partial class Form1 : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        private readonly Client client;

        public Form1()
        {
            InitializeComponent();

            client = new Client();
            client.Connect();

            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));

            pnlNav.Height = btnLoginNav.Height;
            pnlNav.Top = btnLoginNav.Top;

            btnLoginNav.BackColor = Color.FromArgb(46, 51, 73);

            tbEmail.Visible = false;
            lblEmail.Visible = false;
            btnCreateAccount.Visible = false;
            btnLogin.Visible = true;

            tbUsername.Text = "";
            tbPassword.Text = "";
            tbEmail.Text = "";

            lblSuccess.Text = "";
            lblSuccess.Visible = false;
            lblError.Text = "";
            lblError.Visible = false;
        }

        private void btnLoginNav_Click(object sender, EventArgs e)
        {
            pnlNav.Height = btnLoginNav.Height;
            pnlNav.Top = btnLoginNav.Top;

            btnLoginNav.BackColor = Color.FromArgb(46, 51, 73);
            btnCreateAccountNav.BackColor = Color.FromArgb(24, 30, 54);

            tbEmail.Visible = false;
            lblEmail.Visible = false;
            btnCreateAccount.Visible = false;
            btnLogin.Visible = true;

            tbUsername.Text = "";
            tbPassword.Text = "";
            tbEmail.Text = "";

            lblSuccess.Text = "";
            lblSuccess.Visible = false;
            lblError.Text = "";
            lblError.Visible = false;
        }

        private void btnLogin_Leave(object sender, EventArgs e)
        {
            btnLoginNav.BackColor = Color.FromArgb(24, 30, 54);
        }

        private void btnCreateAccountNav_Click(object sender, EventArgs e)
        {
            pnlNav.Height = btnCreateAccountNav.Height;
            pnlNav.Top = btnCreateAccountNav.Top;

            btnLoginNav.BackColor = Color.FromArgb(24, 30, 54);
            btnCreateAccountNav.BackColor = Color.FromArgb(46, 51, 73);

            tbEmail.Visible = true;
            lblEmail.Visible = true;
            btnCreateAccount.Visible = true;
            btnLogin.Visible = false;

            tbUsername.Text = "";
            tbPassword.Text = "";
            tbEmail.Text = "";

            lblSuccess.Text = "";
            lblSuccess.Visible = false;
            lblError.Text = "";
            lblError.Visible = false;
        }

        private void btnCreateAccount_Leave(object sender, EventArgs e)
        {
            btnCreateAccountNav.BackColor = Color.FromArgb(24, 30, 54);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private static readonly string sDllPath = "ijl15.dll";
        private static readonly uint CREATE_SUSPENDED = 0x00000004;

        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        public struct SECURITY_ATTRIBUTES
        {
            public int length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;

        }

        [DllImport("kernel32.dll")]
        static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        private void btnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;

            STARTUPINFO si = new STARTUPINFO();

            string text1 = tbUsername.Text;
            string text2 = tbPassword.Text;

            tbPassword.Clear();

            string token = GetToken(text1, text2);
            bool flag = !token.Equals("");

            if (flag)
            {
                try
                {
                    bool bCreateProc = CreateProcess("MapleStory.exe", $" WebStart {token}", IntPtr.Zero, IntPtr.Zero, false, CREATE_SUSPENDED, IntPtr.Zero, null, ref si, out PROCESS_INFORMATION pi);

                    if (bCreateProc)
                    {
                        int bInject = Inject("MapleStory", sDllPath);

                        if (bInject == 0)
                        {
                            ResumeThread(pi.hThread);

                            CloseHandle(pi.hThread);
                            CloseHandle(pi.hProcess);

                            lblSuccess.Text = "MapleStory launched!";
                            lblSuccess.Visible = true;
                            lblError.Text = "";
                            lblError.Visible = false;
                        }
                        else
                        {
                            lblSuccess.Text = "";
                            lblSuccess.Visible = false;
                            lblError.Text = $"Error code: {bInject}";
                            lblError.Visible = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblSuccess.Text = "";
                    lblSuccess.Visible = false;
                    lblError.Text = "Could not start! Make sure the file is in your game folder and that this program is ran as admin.";
                    lblError.Visible = true;

                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                lblSuccess.Text = "";
                lblSuccess.Visible = false;
                lblError.Text = "Invalid username/password combination.";
                lblError.Visible = true;
            }

            btnLogin.Enabled = true;
        }

        private string GetToken(string username, string pwd)
        {
            client.Send(OutPackets.AuthRequest(username, pwd));

            return Handlers.getAuthTokenFromInput(client.Receive());
        }

        private static int Inject(string exe, string dllPath)
        {
            int processID = GetProcessId(exe);

            if (processID == -1)
                return 1;

            IntPtr pLoadLibraryAddress = GetProcAddress(GetModuleHandle("Kernel32.dll"), "LoadLibraryA");

            if (pLoadLibraryAddress == (IntPtr)0)
                return 2;

            IntPtr processHandle = OpenProcess((0x2 | 0x8 | 0x10 | 0x20 | 0x400), 1, (uint)processID);

            if (processHandle == (IntPtr)0)
                return 3;

            IntPtr lpAddress = VirtualAllocEx(processHandle, (IntPtr)null, (IntPtr)dllPath.Length, (0x1000 | 0x2000), 0X40);

            if (lpAddress == (IntPtr)0)
                return 4;

            byte[] bytes = Encoding.ASCII.GetBytes(dllPath);

            if (WriteProcessMemory(processHandle, lpAddress, bytes, (uint)bytes.Length, 0) == 0)
                return 5;

            if (CreateRemoteThread(processHandle, (IntPtr)null, (IntPtr)0, pLoadLibraryAddress, lpAddress, 0, (IntPtr)null) == (IntPtr)0)
                return 6;

            CloseHandle(processHandle);

            return 0;
        }

        private static int GetProcessId(string proc)
        {
            int processID = -1;
            Process[] processes = Process.GetProcesses();

            for (int i = 0; i < processes.Length; i++)
            {
                if (processes[i].ProcessName == proc)
                {
                    processID = processes[i].Id;
                    break;
                }
            }

            return processID;
        }

        private void btnCreateAccount_Click(object sender, EventArgs e)
        {
            btnCreateAccount.Enabled = false;

            string username = tbUsername.Text;
            string password = tbPassword.Text;
            string email = tbEmail.Text;

            tbPassword.Text = "";

            if (username.Length < 4)
            {
                lblSuccess.Text = "";
                lblSuccess.Visible = false;
                lblError.Text = "Username must be at least 4 characters long.";
                lblError.Visible = true;
            }
            else if (password.Length < 6)
            {
                lblSuccess.Text = "";
                lblSuccess.Visible = false;
                lblError.Text = "Password must be at least 6 characters long.";
                lblError.Visible = true;
            }
            else if (email == null || email.Equals("") || !IsValid(email))
            {
                lblSuccess.Text = "";
                lblSuccess.Visible = false;
                lblError.Text = "Email is invalid.";
                lblError.Visible = true;
            }
            else
            {
                CreateAccount(username, password, email);
            }

            btnCreateAccount.Enabled = true;
        }

        private bool IsValid(string email)
        {
            try
            {
                MailAddress mailAddress = new MailAddress(email);

                return true;
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex.Message);

                return false;
            }
        }

        private bool CreateAccount(string username, string password, string email)
        {
            byte request = SendAccountCreateRequest(username, password, email);

            switch (request)
            {
                case 0:
                    lblSuccess.Text = "Account successfully created!";
                    lblSuccess.Visible = true;
                    lblError.Text = "";
                    lblError.Visible = false;
                    break;
                case 1:
                    lblSuccess.Text = "";
                    lblSuccess.Visible = false;
                    lblError.Text = "Account name already taken!";
                    lblError.Visible = true;
                    break;
                case 2:
                    lblSuccess.Text = "";
                    lblSuccess.Visible = false;
                    lblError.Text = "This IP has already created an account!";
                    lblError.Visible = true;
                    break;
                case 3:
                    lblSuccess.Text = "";
                    lblSuccess.Visible = false;
                    lblError.Text = "Unknown error: Client and server info are mismatched!";
                    lblError.Visible = true;
                    break;
            }

            return request == 0;
        }

        private byte SendAccountCreateRequest(string username, string password, string email)
        {
            client.Send(OutPackets.CreateAccountRequest(username, password, email));

            InPacket inPacket = client.Receive();
            inPacket.readInt();
            inPacket.readShort();

            return inPacket.readByte();
        }
    }
}