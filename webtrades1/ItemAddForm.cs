using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace webtrades1
{
    public partial class ItemAddForm : Form
    {
        webcontext db;
        public ItemAddForm()//Загружаем данные из бд
        {
            InitializeComponent();
            db = new webcontext();
            db.Items.Load();//загружаем данные по товарам
            Check();//Проверка при запуске формы
        }

        private void textBox1_TextChanged(object sender, EventArgs e)//При изменении названия товара делаем проверку
        {
            Check();
        }
        private void Check()//Проверка введнных данных на правильность
        {
            if(textBox1.Text.Length<3 || textBox1.Text.Length >20)//Проверка названия товара
            {
                textBox3.Text = "Введите название товара: от 3 до 20 символов";
                textBox3.Visible = true;
                button1.Enabled = false;
            }
            else
            {
                Item item = new Item();
                item = db.Items.FirstOrDefault(u => u.Name == textBox1.Text);
                if (item != null)//Если товар с таким названием уже есть
                {
                    textBox3.Text = "Товар с таким именем уже существует";
                    textBox3.Visible = true;
                    button1.Enabled = false;
                }
                else
                {
                    double rt;
                    bool converted = Double.TryParse(textBox2.Text.ToString(), out rt);
                    if (converted == false)//Если курс товара некорректен
                    {
                        textBox3.Text = "Введите курс в виде числа с плавающей запятой";
                        textBox3.Visible = true;
                        button1.Enabled = false;
                    }
                    if (rt <= 0 || rt >= double.MaxValue)//Если курс твоара меньше 0
                    {
                        textBox3.Text = "Курс должен быть больше 0";
                        textBox3.Visible = true;
                        button1.Enabled = false;
                    }
                    else
                    {
                        textBox3.Visible = false;
                        button1.Enabled = true;
                    }
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)//При изменении курса товара делаем проверку
        {
            Check();
        }

        private void ItemAddForm_Load(object sender, EventArgs e)
        {

        }
    }
}
