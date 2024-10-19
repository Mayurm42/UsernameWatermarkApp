using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Security.Principal; // Namespace for WindowsIdentity

namespace UsernameWatermarkApp
{
    public partial class Form1 : Form
    {
        // Import necessary DLLs to make the window click-through
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int GWL_EXSTYLE = -20;

        public Form1()
        {
            InitializeComponent();
            // Set the form to full screen
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;  // Always on top
            this.BackColor = Color.White;
            this.TransparencyKey = Color.White;  // Make the form transparent
            this.Opacity = 0.8;  // Set opacity to 40% for visibility
            this.ShowInTaskbar = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            AddRepeatingWatermark(e.Graphics);
        }

        private void AddRepeatingWatermark(Graphics g)
        {
            string emailAlias = GetLoggedInUserEmail(); // Get the logged-in user's email alias
            string dateStamp = DateTime.Now.ToString("dd-MM-yyyy"); // Current date in dd-MM-yyyy format
            string timeStamp = DateTime.Now.ToString("HH:mm:ss"); // Current time in HH:mm:ss format
            string dateTimeStamp = $"{dateStamp} {timeStamp}"; // Combine date and time

            using (Font watermarkFont = new Font("Segoe UI", 20, FontStyle.Regular)) // Font size changed to 20
            {
                // Light blue semi-transparent color
                Color watermarkColor = Color.FromArgb(128, 173, 216, 230); // Light blue

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Calculate the diagonal step size (spacing between repeated watermarks)
                int step = 400;  // Increased spacing between watermarks
                int watermarkWidthEmail = (int)g.MeasureString(emailAlias, watermarkFont).Width;
                int watermarkWidthDateTime = (int)g.MeasureString(dateTimeStamp, watermarkFont).Width; // Measure combined date and time
                int watermarkHeight = (int)g.MeasureString(emailAlias, watermarkFont).Height;

                // Loop to draw watermarks diagonally across the screen
                for (int x = -this.Width; x < this.Width * 2; x += step)
                {
                    for (int y = -this.Height; y < this.Height * 2; y += step)
                    {
                        // Save current graphics state before rotation
                        g.TranslateTransform(x + Math.Max(watermarkWidthEmail, watermarkWidthDateTime) / 2, y + watermarkHeight / 2);
                        g.RotateTransform(-35);  // Rotate by 35 degrees

                        // Draw the email alias
                        g.DrawString(emailAlias, watermarkFont, new SolidBrush(watermarkColor), -watermarkWidthEmail / 2, -watermarkHeight / 2);

                        // Draw the combined date and time stamp below the email alias
                        g.DrawString(dateTimeStamp, watermarkFont, new SolidBrush(watermarkColor), -watermarkWidthDateTime / 2, watermarkHeight / 2 + 20); // Offset slightly down

                        // Reset transformation
                        g.ResetTransform();
                    }
                }
            }
        }

        private string GetLoggedInUserEmail()
        {
            string emailAlias = string.Empty;

            // Retrieve the user's UserPrincipalName (email) directly
            emailAlias = WindowsIdentity.GetCurrent().Name;

            // If the format is 'DOMAIN\username', convert to 'username@domain.com'
            string[] parts = emailAlias.Split('\\');
            if (parts.Length > 1)
            {
                string username = parts[1];
                string domain = "domain.com"; // Replace with your actual domain
                emailAlias = $"{username}@{domain}";
            }

            return emailAlias;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Invalidate the form to ensure the paint event is called
            this.Invalidate();

            // Make the form click-through
            int initialStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, initialStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }
    }
}
