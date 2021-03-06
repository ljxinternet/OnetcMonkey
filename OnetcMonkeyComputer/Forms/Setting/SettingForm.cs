﻿using OnetcMonkeyComputer.AppServices;
using OnetcMonkeyComputer.Config;
using OnetcMonkeyComputer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnetcMonkeyComputer.Forms.Setting
{
    public partial class SettingForm : Form
    {
        public delegate void GetUserInfo(string token);


        IAppService _appService = new AppService();
        IConfigService _configService = new ConfigService();
        public SettingForm()
        {
            InitializeComponent();
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            textBox_token.Enabled = button_save.Enabled = false;
            var token = textBox_token.Text.Trim();
            if (string.IsNullOrEmpty(token) || token.Length < 20)
            {
                MessageBox.Show("请配置正确的Token字符串！");
                return;
            }

            LoadAndSaveSetting(token);
        }

        private void LoadAndSaveSetting(string token)
        {
            this.Invoke(new GetUserInfo(SaveSetting), new object[] { token });
        }

        private void SaveSetting(string token)
        {
            if(comboBox1.SelectedIndex<0)
            {
                MessageBox.Show("请选择服务器");
                textBox_token.Enabled = button_save.Enabled = true;
                return;
            }

            var server = (ServerInfo)comboBox1.SelectedItem;

            IMonkeyService _monkeyService = new MonkeyService(server);
           
            try
            {
                var user = _monkeyService.GetUserInfo(token);


                if (user == null || user.otc == null)
                {
                    textBox_token.Enabled = button_save.Enabled = true;
                    MessageBox.Show("验证失败，请检查token是否正确！");
                    label_result.Text = "配置失败×";
                    label_result.ForeColor = Color.Red;
                    return;
                }

                var config = _configService.ReadConfig();

                config.Token = token;
                config.Wallet = user.otc;

             
                config.BaseApiUrl = server.BaseApiUrl;
                config.BaseUrl = server.BaseUrl;
                config.ServerName = server.Name;
                config.ServerTag = server.ServerTag;
                _configService.Save(config);

                var hnbc_token = _appService.Login(user.otc, user.nickName);
                textBox_token.Enabled = button_save.Enabled = true;
                if (hnbc_token == null)
                {
                    MessageBox.Show("连接服务器失败，部分功能将无法使用！");

                    label_result.Text = "配置失败×";
                    label_result.ForeColor = Color.Red;
                    return;
                }

                config.ServerToken = hnbc_token.accessToken;
                config.UserConfig = hnbc_token.userConfig;
                _configService.Save(config);

                MessageBox.Show("配置成功！请重新打开软件！");
                label_result.Text = "配置成功√";
                label_result.ForeColor = Color.Lime;
            }catch(Exception ex)
            {
                textBox_token.Enabled = button_save.Enabled = true;
                label_result.Text = "配置失败×";
                label_result.ForeColor = Color.Red;
                MessageBox.Show("配置失败，原因："+ex.Message);
            }
        }

        private void SettingForm_Load(object sender, EventArgs e)
        { 
            var config = _configService.ReadConfig();
            textBox_token.Text = config.Token;
            
            List<ServerInfo> servers = _configService.ReadMonkeyServers();

            comboBox1.DataSource = servers;
            comboBox1.DisplayMember = "Name";


            var server = servers.Where(w=>w.Name == config.ServerName).FirstOrDefault();
            var index = servers.IndexOf(server);
            if(index>=0)
            {
                comboBox1.SelectedIndex = index;
            }

        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("http://www.onetc.info/wkmonkey/hai-na-bai-chuan-wan-ke-hou-fu-zhu-gong-ju-3-0-fa-bu-");
            } catch  
            { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var servers = _appService.GetServerList();
            _configService.SaveMonkeyServers(servers);
             
            comboBox1.DataSource = servers;
            comboBox1.DisplayMember = "Name";
        }
    }
}
