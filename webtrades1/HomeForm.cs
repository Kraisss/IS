using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.WinForms;
using LiveCharts.Wpf;

namespace webtrades1
{
    public partial class HomeForm : Form
    {
        webcontext db;
        public HomeForm()
        {
            InitializeComponent();
            db = new webcontext();
            db.Items.Load();
            db.ExchangeRateHistories.Load();
            db.People.Load();
            db.ItemPersonAccounts.Load();
            db.Roles.Load();
            dataGridView1.DataSource = db.Items.Local.ToBindingList();//Загружаем данные по товарам в таблицу при просмотре истории курса
            dataGridView4.DataSource = db.Items.Local.ToBindingList();//Загружаем данные по товарам в таблицу при формировании отчетов
            IQueryable<Person> users = db.People.Where(p => p.role.Name=="User").OrderByDescending(p => p.Id);
            dataGridView3.DataSource = users.ToList();//Загружаем список User-ов из бд и заносим в таблицу пользователей при формировании отчетов
            dateTimePicker2.MinDate = dateTimePicker1.Value;//Для выбора промежутка поиска операций в формировании отчетов
        }

        private void HomeForm_Load(object sender, EventArgs e)
        {
            
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)//Прячем ненужные столбцы
        {
            this.dataGridView1.Columns["RateHistory"].Visible = false;
            this.dataGridView1.Columns["Id"].Visible = false;
        }

        private void dataGridView2_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)//Прячем ненужные столбцы
        {
            this.dataGridView2.Columns["Item"].Visible = false;
            this.dataGridView2.Columns["ItemId"].Visible = false;
            this.dataGridView2.Columns["Id"].Visible = false;
        }
        private void GraphBuild()//Строим график
        {
            int index = dataGridView1.SelectedRows[0].Index;//находим по индексу выбранной строки элемент в базе данных
            int id = 0;
            bool converted = Int32.TryParse(dataGridView1[0, index].Value.ToString(), out id);
            if (converted == false)
                return;

            Item item = db.Items.Find(id);//находим по найденному товару истории изменения курса
            IQueryable<ExchangeRateHistory> histories = db.ExchangeRateHistories.Where(p => p.ItemId == item.Id).OrderBy(p => p.Id);
            List<ExchangeRateHistory> list = histories.ToList();

            SeriesCollection series = new SeriesCollection();//создаем график истории изменения курса товара
            ChartValues<double> ratevalues = new ChartValues<double>();
            List<string> dates = new List<string>();
            foreach(ExchangeRateHistory h in list)
            {
                ratevalues.Add(h.ExchangeRateChange);
                dates.Add(h.DateOfChange.ToString());
            }
            cartesianChart1.AxisX.Clear();
            cartesianChart1.AxisX.Add(new Axis() 
            {
            Title="Дата",
            Labels=dates
            });
            LineSeries line = new LineSeries();
            line.Title = item.Name;
            line.Values = ratevalues;
            series.Add(line);
            cartesianChart1.Series = series;
            cartesianChart1.LegendLocation = LegendLocation.Bottom;
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)// При выборе другого товара загружаем его изменения курса
        {
            if (dataGridView1.SelectedRows.Count > 0)//берем выбранную строку
            {
                int index = dataGridView1.SelectedRows[0].Index;//находим по ее индексу элемент в базе данных
                int id = 0;
                bool converted = Int32.TryParse(dataGridView1[0, index].Value.ToString(), out id);
                if (converted == false)
                    return;

                Item item = db.Items.Find(id);//находим историю изменения курса для выбранного товара
                IQueryable<ExchangeRateHistory> histories = db.ExchangeRateHistories.Where(p => p.ItemId == item.Id).OrderByDescending(p=>p.Id);
                dataGridView2.DataSource = histories.ToList();
                GraphBuild();// строим график заново
            } 
        }
        private void Updater()// Обновляем историю курса товара
        {
            int index = dataGridView1.SelectedRows[0].Index;//находим элемент по индексу строки таблицы в базе данных
            int id = 0;
            bool converted = Int32.TryParse(dataGridView1[0, index].Value.ToString(), out id);
            if (converted == false)
                return;

            Item item = db.Items.Find(id);//находим по выбранному товару историю курса и загружаем в таблицу
            IQueryable<ExchangeRateHistory> histories = db.ExchangeRateHistories.Where(p => p.ItemId == item.Id).OrderByDescending(p => p.Id);
            dataGridView2.DataSource = histories.ToList();
        }
        private void Updater3()// Проверяем уровень доступа пользователя и заполняем соотв. столбец в таблице
        {
            
            for (int i = 0; i < dataGridView3.Rows.Count; i++)
            {

                string temp = dataGridView3.Rows[i].Cells["Level"].Value.ToString();
               
                switch (temp)
                {
                    case "0":
                        
                        dataGridView3.Rows[i].Cells["Access"].Value = "Denied";
                        break;
                    case "1":
                        
                        dataGridView3.Rows[i].Cells["Access"].Value = "Allowed";
                        break;
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)//Кнопка добавления товара
        {
            ItemAddForm form = new ItemAddForm();//Вызываем форму
            
            DialogResult result = form.ShowDialog(this);
            if (result == DialogResult.Cancel)//После закрытия формы заносим полученные данные в базу
                return;
            Item item = new Item();

            item.Name = form.textBox1.Text;//Создаем товар по введенным данным
            item.ExchangeRate = Convert.ToDouble(form.textBox2.Text);
            ExchangeRateHistory erh = new ExchangeRateHistory();//Делаем запись об изменении курса товара
            erh.DateOfChange = DateTime.Now;
            erh.ExchangeRateChange = item.ExchangeRate;
            erh.Item = item;
            item.RateHistory.Add(erh);
            
            db.Items.Add(item);//Сохраняем запись и товар в бд
            db.ExchangeRateHistories.Add(erh);
            db.SaveChanges();
            List<Person> users = db.People.Local.Where(u=>u.role.Name=="User").ToList();//для каждого юзера создаем баланс этого товара в бд
            if (users != null)
            {
                foreach (Person v in users)
                {
                    ItemPersonAccount ipa = new ItemPersonAccount();
                    ipa.Item = item;
                    ipa.ItemQuantity = 0;
                    ipa.Person = v;
                    ipa.PersonId = v.Id;
                    db.ItemPersonAccounts.Add(ipa);
                    v.Accounts.Add(ipa);
                    db.SaveChanges();

                }
            }
            dataGridView1.Refresh();//Обновляем таблицу товаров 
            Updater();//Обновляем таблицу истории курса
            GraphBuild();//Заново строим график

        }

        private void button4_Click(object sender, EventArgs e)//Кнопка изменения курса товара
        {
            if (dataGridView1.SelectedRows.Count > 0)//берем выбранную строку
            {
                int index = dataGridView1.SelectedRows[0].Index;//находим по ее индексу элемент в базе данных
                int id = 0;
                bool converted = Int32.TryParse(dataGridView1[0, index].Value.ToString(), out id);
                if (converted == false)
                    return;
                Item item = db.Items.Find(id);//находим товар и загружаем его данные в форму изменения курса
                CourseChangeForm form = new CourseChangeForm();
                form.textBox2.Text = item.Name;
                form.textBox3.Text = item.ExchangeRate.ToString();
                DialogResult result = form.ShowDialog(this);
                if (result == DialogResult.Cancel)//После закрытия формы заносим полученные данные в базу
                    return;
                item.ExchangeRate = Convert.ToDouble(form.textBox4.Text);//Меняем курс товара
                db.SaveChanges();
                ExchangeRateHistory erh = new ExchangeRateHistory();//Делаем запись об изменении курса товара
                erh.Item = item;
                erh.ItemId = item.Id;
                erh.ExchangeRateChange = item.ExchangeRate;
                erh.DateOfChange = DateTime.Now;
                item.RateHistory.Add(erh);
                db.ExchangeRateHistories.Add(erh);//Сохраняем изменения в бд
                db.SaveChanges();
                dataGridView1.Refresh();
                Updater();//Обновляем таблицу курса товара
                GraphBuild();//Заново строим график

            }
            

        }

        private void dataGridView3_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)//Прячем ненужные столбцы
        {
            this.dataGridView3.Columns["Accounts"].Visible = false;
            this.dataGridView3.Columns["Operations"].Visible = false;
            this.dataGridView3.Columns["PasswordHash"].Visible = false;
            this.dataGridView3.Columns["PasswordSalt"].Visible = false;
            this.dataGridView3.Columns["RoleId"].Visible = false;
            this.dataGridView3.Columns["role"].Visible = false;
            this.dataGridView3.Columns["Id"].Visible = false;
            this.dataGridView3.Columns["Level"].Visible = false;
            Updater3();
        }

        private void dataGridView3_SelectionChanged(object sender, EventArgs e)// При выборе другого пользователя проверяем его доступ и меняем текст кнопки для изменения доступа
        {
            if (dataGridView3.SelectedRows.Count > 0)//берем выбранную строку
            {
                int index = dataGridView3.SelectedRows[0].Index;//находим по ее индексу элемент в базе данных
                int id = 0;
                bool converted = Int32.TryParse(dataGridView3[0, index].Value.ToString(), out id);
                if (converted == false)
                    return;

                Person person = db.People.Find(id);//для найденного пользователя проверяем уровень доступа
                if (person.Level == 0)//В каждом из двух случаев меняем надпись на кнопке управления доступом
                {
                    button2.Text = "Разблокировать";
                }
                else
                {
                    button2.Text = "Заблокировать";
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)// Кнопка изменения доступа выбранного User-a
        {
            if (dataGridView3.SelectedRows.Count > 0)//берем выбранную строку
            {
                int index = dataGridView3.SelectedRows[0].Index;//находим по ее индексу элемент в базе данных
                int id = 0;
                bool converted = Int32.TryParse(dataGridView3[0, index].Value.ToString(), out id);
                if (converted == false)
                    return;

                Person person = db.People.Find(id);//Для найденного пользователя проверяем уровень доступа
                if (person.Level == 0)//Меняем уровень доступа (для каждого случая) на противоположный и меняем надпись на кнопке
                {
                    person.Level = 1;
                    db.SaveChanges();
                    dataGridView3.Refresh();
                    button2.Text = "Заблокировать";
                    Updater3();
                }
                else
                {
                    person.Level = 0;
                    db.SaveChanges();
                    dataGridView3.Refresh();
                    button2.Text = "Разблокировать";
                    Updater3();
                }
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)//Поле для поиска User-a по логину
        {
            if(String.IsNullOrEmpty(textBox3.Text))//Если поле пустое загружаем всех User-ов
            {
                IQueryable<Person> users = db.People.Where(p => p.role.Name == "User").OrderByDescending(p => p.Id);
                dataGridView3.DataSource = users.ToList();
            }
            else//Иначе загружаем User-ов с таким логином
            {
                IQueryable<Person> users = db.People.Where(p => p.role.Name == "User"&& p.Login.Contains(textBox3.Text)).OrderByDescending(p => p.Id);
                dataGridView3.DataSource = users.ToList();
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)//Меняем мин. зн. второго dateTimePicker на выбранное значение первого
        {
            dateTimePicker2.MinDate = dateTimePicker1.Value;
        }

        private void button5_Click(object sender, EventArgs e)// Кнопка формирования отчета для определенного пользователя
        {
            if (dataGridView3.SelectedRows.Count > 0)//берем выбранную строку
            {
                int index = dataGridView3.SelectedRows[0].Index;//находим по ее индексу элемент в базе данных
                int id = 0;
                bool converted = Int32.TryParse(dataGridView3[0, index].Value.ToString(), out id);
                if (converted == false)
                    return;

                Person person = db.People.Find(id);// Находим User-a по выбранной строке
                if(radioButton1.Checked)//Формируем отчет по операциям за все время
                {
                    List<TradeOperation> operations = new List<TradeOperation>();
                    operations = db.TradeOperations.Where(u => u.PersonId == person.Id).ToList();//Находим все операции для пользователя
                    string s = $"{DateTime.Now.ToLocalTime()}\r\nБыло найдено {operations.Count} операций  совершенных пользователем {person.Login} за все время";
                    if (operations.Count != 0)//Если были найдены операции
                    {
                        foreach (TradeOperation t in operations)//Выводим информацию по каждой операции
                        {
                            if (t.OperationType == "Buy")
                            {
                                s += $"\r\nДата проведения:{t.DateOfOperation}; Товар:{t.Item.Name};  Тип операции:{t.OperationType}; Полученный баланс:-{t.Profit}";
                            }
                            else
                            {
                                s += $"\r\nДата проведения:{t.DateOfOperation}; Товар:{t.Item.Name};  Тип операции:{t.OperationType}; Полученный баланс:+{t.Profit}";
                            }
                        }
                        textBox2.Text = s;
                    }
                    else//Выводим что операций не найдено
                    {
                        textBox2.Text = s;
                    }
                }
                else//Формируем отчет по операциям за промежуток времени
                {
                    List<TradeOperation> operations = new List<TradeOperation>();
                    operations = db.TradeOperations.Where(u => u.PersonId == person.Id && u.DateOfOperation>=dateTimePicker1.Value && u.DateOfOperation<=dateTimePicker2.Value).ToList();//Находим операции для пользователя за промежуток времени
                    string s = $"{DateTime.Now.ToLocalTime()}\r\nБыло найдено {operations.Count} операций  совершенных пользователем {person.Login} в период с {dateTimePicker1.Value} по {dateTimePicker2.Value}";
                    if (operations.Count != 0)//Если были найдены операции
                    {
                        foreach (TradeOperation t in operations)//Для каждой операции выводим информацию
                        {
                            if (t.OperationType == "Buy")
                            {
                                s += $"\r\nДата проведения:{t.DateOfOperation}; Товар:{t.Item.Name};  Тип операции:{t.OperationType}; Полученный баланс:-{t.Profit}";
                            }
                            else
                            {
                                s += $"\r\nДата проведения:{t.DateOfOperation}; Товар:{t.Item.Name};  Тип операции:{t.OperationType}; Полученный баланс:+{t.Profit}";
                            }
                        }
                        textBox2.Text = s;
                    }
                    else//Не было найдено операций за промежуток
                    {
                        textBox2.Text = s;
                    }
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)//Кнопка очистки поля отчета от данных
        {
            textBox2.Clear();
        }

        private void button7_Click(object sender, EventArgs e)//Кнопка сохранения отчета в текстовый файл
        {
            DirectoryInfo di = new DirectoryInfo(@"C:\OtchetyWebTrades");//Проверяем наличие папки для сохранения отчетов
            if(!di.Exists)//Если ее нет - создаем
            {
                di.Create();
            }
            string writepath = @"C:\OtchetyWebTrades\ОтчетЗа" + DateTime.Now.ToShortDateString() + ".txt";//Формируем отчет
            MessageBox.Show(writepath);
            try
            {
                using (StreamWriter sw = new StreamWriter(writepath, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(textBox2.Text);
                }
                MessageBox.Show("Отчет создан");
            }
            catch (Exception g)
            {
                MessageBox.Show(g.Message);
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)//Поле поиска товара по названию
        {
            if (String.IsNullOrEmpty(textBox4.Text))//Если поле пустое выводим все товары
            {
                IQueryable<Item> items = db.Items.OrderByDescending(p => p.Id);
                dataGridView4.DataSource = items.ToList();
            }
            else//Выводим товар с введенным названием
            {
                IQueryable<Item> items = db.Items.Where(p =>  p.Name.Contains(textBox4.Text)).OrderByDescending(p => p.Id);
                dataGridView4.DataSource = items.ToList();
            }
        }

        private void dataGridView4_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)//Прячем ненужные столбцы
        {
            this.dataGridView4.Columns["RateHistory"].Visible = false;
            this.dataGridView4.Columns["Id"].Visible = false;
        }

        private void button8_Click(object sender, EventArgs e)//Формирование отчетов по выбранному товару
        {
            if (dataGridView4.SelectedRows.Count > 0)//берем выбранную строку
            {
                int index = dataGridView4.SelectedRows[0].Index;//находим по ее индексу элемент в базе данных
                int id = 0;
                bool converted = Int32.TryParse(dataGridView4[0, index].Value.ToString(), out id);
                if (converted == false)
                    return;

                Item item = db.Items.Find(id);//Формируем отчет по найденному товару
                if (radioButton1.Checked)//формируем отчет по операциям за все время
                {
                    List<TradeOperation> operations = new List<TradeOperation>();
                    operations = db.TradeOperations.Where(u => u.ItemId==item.Id).ToList();//Находим операции по выбранному товару
                    string s = $"{DateTime.Now.ToLocalTime()}\r\nБыло найдено {operations.Count} операций  совершенных по товару {item.Name} за все время";
                    if (operations.Count != 0)//Если были найдены операции
                    {
                        foreach (TradeOperation t in operations)//Для каждой операции выводим информацию
                        {
                            if (t.OperationType == "Buy")
                            {
                                s += $"\r\nПользователь: {t.Person.Login}; Дата проведения:{t.DateOfOperation}; Тип операции:{t.OperationType}; Полученный баланс:-{t.Profit}";
                            }
                            else
                            {
                                s += $"\r\nПользователь: {t.Person.Login}; Дата проведения:{t.DateOfOperation}; Тип операции:{t.OperationType}; Полученный баланс:+{t.Profit}";
                            }
                        }
                        textBox2.Text = s;
                    }
                    else//Операций не было найдено
                    {
                        textBox2.Text = s;
                    }
                }
                else//Формируем отчет за промежуток времени
                {
                    List<TradeOperation> operations = new List<TradeOperation>();
                    operations = db.TradeOperations.Where(u => u.ItemId==item.Id && u.DateOfOperation >= dateTimePicker1.Value && u.DateOfOperation <= dateTimePicker2.Value).ToList();//Находим операции по товару за промежуток времени
                    string s = $"{DateTime.Now.ToLocalTime()}\r\nБыло найдено {operations.Count} операций  совершенных по товару {item.Name} в период с {dateTimePicker1.Value} по {dateTimePicker2.Value}";
                    if (operations.Count != 0)//Если были найдены операции
                    {
                        foreach (TradeOperation t in operations)//Для каждой операции выводим информацию
                        {
                            if (t.OperationType == "Buy")
                            {
                                s += $"\r\nПользователь: {t.Person.Login}; Дата проведения:{t.DateOfOperation}; Тип операции:{t.OperationType}; Полученный баланс:-{t.Profit}";
                            }
                            else
                            {
                                s += $"\r\nПользователь: {t.Person.Login}; Дата проведения:{t.DateOfOperation}; Тип операции:{t.OperationType}; Полученный баланс:+{t.Profit}";
                            }
                        }
                        textBox2.Text = s;
                    }
                    else//Операций не было найдено
                    {
                        textBox2.Text = s;
                    }
                }
            }
        }
    }
}
