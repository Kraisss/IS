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
    public partial class Form1 : Form
    {
        webcontext db;
        MyContextInitializer cont;
        public Form1()//загрузка данных из бд
        {
            cont = new MyContextInitializer();
            db = new webcontext();
            cont.InitializeDatabase(db);
            
            db.People.Load();// Загрузка пользователей из бд
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            
        }

        private void button1_Click(object sender, EventArgs e)//Кнопка авторизации
        {
            Person person = db.People.Include(u=>u.role).FirstOrDefault(u => u.Login == textBox1.Text);//Если находится пользователь с таким логином
            if(person!=null)//То хэшируем введенный пароль с солью и сравниваем его с хэшэм из бд 
            {
                string str1 = PasswordHash.GetHash(textBox2.Text + person.PasswordSalt);
                if(str1==person.PasswordHash)
                {
                    if(person.role.Name!="Admin")//Если хэши совпали проверяем роль пользователя, User-ам вход закрыт
                    {
                        textBox3.Text = "Users are not allowed";
                        textBox3.Visible = true;
                    }
                    else//Если авторизация успешна вызываем главную форму
                    {
                        HomeForm form = new HomeForm();
                        form.textBox1.Text += person.Login;//Загружаем логин пользователя в поле главной формы
                        this.Hide();//Прячем форму авторизации
                        DialogResult result = form.ShowDialog(this);
                        if (result == DialogResult.Abort)//Если пользователь нажал "Выйти из аккаунта" снова выводим форму авторизации
                        {
                            this.Show();
                            return;
                        } 
                        if (result == DialogResult.Cancel)//Если пользователь закрыл приложение, закрываем форму
                            this.Close();
                    }
                }
                else
                {
                    textBox3.Text = "Неверные логин или пароль";
                    textBox3.Visible = true;
                }
            }
            else
            {
                textBox3.Text = "Неверные логин или пароль";
                textBox3.Visible = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)//Вызов формы регистрации
        {
            RegisterForm form = new RegisterForm();//Вызываем форму регистрации
            DialogResult result = form.ShowDialog(this);
            if (result == DialogResult.Cancel)//После закрытия формы заносим полученные данные в базу
                return;
            Role role = db.Roles.FirstOrDefault(u => u.Name == "Admin");
            Person person = new Person();//Создаем пользователя с ролью Admin, с введенными при регистрации Login
            string str1 = PasswordHash.CreateSalt();
            person.role = role;
            person.RoleId = role.Id;
            person.Login = form.textBox1.Text;
            person.PasswordHash = PasswordHash.GetHash(form.textBox2.Text + str1);//Пароль хэшируем с солью и вносим соль и хэш в бд
            person.PasswordSalt = str1;
            person.PersonalAccount = 0;
            db.People.Add(person);
            db.SaveChanges();//Сохраняем изменения

        }
    }
}
