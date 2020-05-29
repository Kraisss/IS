using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace webtrades1
{
    public partial class CourseChangeForm : Form
    {
        public CourseChangeForm()
        {
            InitializeComponent();
            Check();//Проводим проверку при запуске формы
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            Check();//Проводим проверку при изменении поля "Новый курс"
        }
        private void Check()//Проверка состоит в проверке правильности введенных данных и вхождение в границы >0
        {
            double rt;
            bool converted = Double.TryParse(textBox4.Text.ToString(), out rt);
            if (converted == false)
            {
                textBox1.Text = "Введите курс в виде числа с плавающей запятой";
                textBox1.Visible = true;
                button1.Enabled = false;
            }
            if (rt <= 0 || rt >= double.MaxValue)
            {
                textBox1.Text = "Курс должен быть больше 0";
                textBox1.Visible = true;
                button1.Enabled = false;
            }
            else
            {
                textBox1.Visible = false;
                button1.Enabled = true;
            }
        }

        private void CourseChangeForm_Load(object sender, EventArgs e)
        {

        }
    }
}
