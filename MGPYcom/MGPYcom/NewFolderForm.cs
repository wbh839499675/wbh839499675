using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MGPYcom
{
    public partial class NewFolderForm : Form
    {
        //当前路径
        private string curPath;

        public string folderName;

        //管理器的主窗体的一个引用
        private Form_main mainForm;


        public NewFolderForm(string curPath, Form_main mainForm)
        {
            InitializeComponent();
            this.curPath = curPath;
            this.mainForm = mainForm;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            string newFileName = txtNewFolderName.Text;

            string newFilePath = Path.Combine(curPath, newFileName);

            //文件名不合法
            if (!IsValidFileName(newFileName))
            {
                MessageBox.Show("文件夹名不能包含下列任何字符:\r\n" + "\t\\/:*?\"<>|", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {

                folderName = mainForm.curFilePathAT + "/"+newFileName;
                //更新文件列表
                //mainForm.ShowFilesList(curPath, false);
                String strCMD = "AT+FSMKDIR=";
                String strGet = "";

                strCMD = strCMD + "\"" + folderName + "\"" + "\r\n";
                mainForm.SendAtCmd_NoSplit(strCMD, ref strGet);

                this.Close();
                mainForm.refreshList();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }





        //检查文件名是否合法,文件名中不能包含字符\/:*?"<>|
        private bool IsValidFileName(string fileName)
        {
            bool isValid = true;

            //非法字符
            string errChar = "\\/:*?\"<>|";

            for (int i = 0; i < errChar.Length; i++)
            {
                if (fileName.Contains(errChar[i].ToString()))
                {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }

    }

}