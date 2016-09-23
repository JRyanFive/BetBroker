using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BcWin.Processor;
using BcWin.Processor.ScanDriver;
using Timer = System.Timers.Timer;

namespace BcWin.Server
{
    public partial class Form2 : Form
    {

        public AutoResetEvent LoginEvent = new AutoResetEvent(false);
        private IbetEngine ibetEngine = new IbetEngine();
        private IbetEngine ibetEngine2 = new IbetEngine();
        private SboEngine sboEngine = new SboEngine();
        private SboEngine sboEngine2 = new SboEngine();
        public Form2()
        {
            InitializeComponent();
            CoreProcessor.InitConfig();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //sboEngine2.StartTest();
            //dataGridView2.DataSource = sboEngine2.LiveMatchOddDatas;

           
             var aaaa = new System.Diagnostics.Stopwatch();
            aaaa.Start();
            ibetEngine.StartScanEngine(eScanType.Live);
            aaaa.Stop();
            var aaaaaaa = aaaa.ElapsedMilliseconds;
            label2.Text = aaaaaaa.ToString();
            //dataGridView1.DataSource = ibetEngine.LiveMatchOddDatas;

            //sboEngine.Login("https://www.currybread.com/", "msn99aa001", "123@@Googlee");
            //sboEngine.StartScanEngine(eScanType.Live);
            //dataGridView1.DataSource = sboEngine.LiveMatchOddDatas;

            try
            {
                //var list = DataContainer.LiveMatchOddBag.Where(m => !m.IsDeleted).ToList();
                //dataGridView1.DataSource = list;

                //label1.Text = list.Count.ToString();



                //var aaaa = new System.Diagnostics.Stopwatch();
                //aaaa.Start();

                //sboEngine2.StartTest1();
                ////var aaa = sboEngine2.LiveMatchOddDatas.First();

                ////var aa1 = sboEngine2.PrepareBet(aaa, eBetType.Away, true);
                ////var aa2 = sboEngine2.ConfirmBet(10, true);
                //aaaa.Stop();
                //var aaaaaaa = aaaa.ElapsedMilliseconds;
                //label2.Text = aaaaaaa.ToString();
                //dataGridView2.DataSource = sboEngine2.LiveMatchOddDatas;
            }
            catch (Exception)
            {
                return;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

           // ibetEngine.Login("http://www.88cado.com/", "TKH19900010", "ABab1212");
            var aaaa = new System.Diagnostics.Stopwatch();
            aaaa.Start();
            ibetEngine.StartScanTEST();
            aaaa.Stop();
            var aaaaaaa = aaaa.ElapsedMilliseconds;
            label1.Text = aaaaaaa.ToString();
            //IbetSboDriver.Start();
            //ibetEngine2.Login("http://www.88cado.com/", "ZW209902020", "ABab1212");
            //ibetEngine2.StartScanEngine(eScanType.Live);
            //dataGridView2.DataSource = ibetEngine2.LiveMatchOddDatas;


            //sboEngine2.StartScanEngine(eScanType.Live);
            ////////try
            ////////{
            ////////    var aaaa = new System.Diagnostics.Stopwatch();
            ////////    aaaa.Start();
            ////////    sboEngine2.StartTest1();
            // sboEngine.StartTest1();
            ////////    aaaa.Stop();
            ////////    var aaaaaaa = aaaa.ElapsedMilliseconds;
            ////////    label1.Text = aaaaaaa.ToString();
            ////////    dataGridView1.DataSource = sboEngine.LiveMatchOddDatas;
            ////////    dataGridView2.DataSource = sboEngine2.LiveMatchOddDatas;
            ////////}
            ////////catch (Exception)
            ////////{
            ////////    return;
            ////////}

            //dataGridView2.DataSource = sboEngine2.LiveMatchOddDatas;
        }
        Custom stcudents = new Custom();
        List<Custom> aaaaaaaaaaa = new List<Custom>();

        private int ii = 1;
        private void WaitScanCallback(object obj)
        {
            ii++;
            Task.Run(() =>
            {
                var s = stcudents.students.FirstOrDefault(a => a.Id == 1);
                s.Name = "DFE " + ii;
            });
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            var dati = DateTime.Now;

            var add = DateTime.Now;

            var bbb = (add - dati).Seconds;

            ibetEngine.Login("http://www.88cado.com/", "TKH19900010", "ABab1212");
            

            // sboEngine2.Login("https://www.currybread.com/", "msn99aa001", "123@@Googlee");

            //sboEngine2.Login("https://www.currybread.com/", "msn99aa002", "123@@Googlee");
            //aaaaaaaaaaa.Add(new Custom("aaaaaaabd", 5000, aaaaaaaaaaa));
            //aaaaaaaaaaa.Add(new Custom("bbbbb", 15000, aaaaaaaaaaa));
            //aaaaaaaaaaa.Add(new Custom("ccccccc", 60000, aaaaaaaaaaa));
        }

        private bool HasOddDefChange(float oldDef, float newDef, eOddType oType)
        {
            return Math.Abs(oldDef) != Math.Abs(newDef);
            //if (oType == eOddType.OU || oType == eOddType.HalfOU 
            //    || (oldDef >= 0 && newDef >= 0) || (oldDef < 0 && newDef < 0))
            //{
            //    return !oldDef.Equals(newDef);
            //}

            //return oldDef + newDef != 0;
        }

    }

    public class Custom
    {
        public List<Student> students = new List<Student>();


        string item; //will hold the item
        Timer timer; //will hanlde the expiry
        List<Custom> refofMainList; //will be used to remove the item once it is expired

        public Custom(string yourItem, int milisec, List<Custom> refOfList)
        {
            students.Add(new Student()
            {
                Id = 1,
                Name = "ABC1"
            });

            students.Add(new Student()
            {
                Id = 2,
                Name = "ABC2"
            });
        }

        public Custom()
        {
            students.Add(new Student()
            {
                Id = 1,
                Name = "ABC1"
            });

            students.Add(new Student()
            {
                Id = 2,
                Name = "ABC2"
            });
        }

        private void Elapsed_Event(object sender, ElapsedEventArgs e)
        {
            timer.Elapsed -= new ElapsedEventHandler(Elapsed_Event);
            refofMainList.Remove(this);

        }
    }

    public class Student
    {
        public string Name;
        public int Id;
    }
}
