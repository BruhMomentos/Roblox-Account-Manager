﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using Cookie = CefSharp.Cookie;

namespace RBX_Alt_Manager
{
    public partial class AccountAdder : Form
    {
        private delegate void SafeCallDelegate();
        private string SecurityToken;

        public AccountAdder()
        {
            InitializeComponent();
            CefSettings settings = new CefSettings();
            // Initialize cef with the provided settings
            Cef.EnableHighDPISupport();
            Cef.Initialize(settings);
            // Create a browser component
            chromeBrowser = new ChromiumWebBrowser("https://roblox.com/login");
            chromeBrowser.AddressChanged += OnNavigated;
            chromeBrowser.FrameLoadEnd += OnPageLoaded;
            // Add it to the form and fill it to the form window.
            Controls.Add(chromeBrowser);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chromeBrowser.Dock = DockStyle.Fill;
        }

        private void CloseForm()
        {
            if (this.InvokeRequired)
            {
                var close = new SafeCallDelegate(CloseForm);
                this.Invoke(close, new object[] { });
            }
            else
                Close();
        }

        public void HideForm()
        {
            if (this.InvokeRequired)
            {
                var hide = new SafeCallDelegate(HideForm);
                this.Invoke(hide, new object[] { });
            }
            else
            {
                Hide();
                ClearData();
                chromeBrowser.Load("https://roblox.com/login");
            }
        }

        public void ShowForm()
        {
            Show();
        }

        public void ClearData()
        {
            Cef.GetGlobalCookieManager().DeleteCookies();
        }

        private async void OnPageLoaded(object sender, FrameLoadEndEventArgs args)
        {
            if (args.Url.Contains("my/account/json"))
            {
                string src = await args.Frame.GetSourceAsync();
                Program.MainForm.GetType().GetMethod("AddAccount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { SecurityToken, src });
                HideForm();
                chromeBrowser.Load("https://roblox.com/login");
                // CloseForm();
            }
        }

        private async void OnNavigated(object sender, AddressChangedEventArgs args)
        {
            string url = args.Address;

            if (url.Contains("/home"))
            {
                var cookieManager = Cef.GetGlobalCookieManager();

                await cookieManager.VisitAllCookiesAsync().ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    List<Cookie> cookies = t.Result;

                    Cookie RSec = cookies.Find(x => x.Name == ".ROBLOSECURITY");

                    if (RSec != null)
                    {
                        SecurityToken = RSec.Value;
                        chromeBrowser.Load("https://www.roblox.com/my/account/json");
                    }
                }
            });
            }
        }

        private void AccountAdder_FormClosing(object sender, FormClosingEventArgs e)
        {
            HideForm();
            e.Cancel = true;
        }
    }
}