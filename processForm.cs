using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Player
{
    public partial class processForm : Form
    {

        private double tempValue = 0;
        public processForm()
        {
            InitializeComponent();
        }

        private void processForm_Load(object sender, EventArgs e)
        {

        }
        public int Addprogess(int sum)
        {
            double score =0;
            score = 100*1.0 / sum;
            if (score >= 1)
            {
                // add num of real score
                int emp = Convert.ToInt32(score);
                if (progressBar1.Value <= 100)
                {
                    if (progressBar1.Value + emp < 100)
                    {                        
                        progressBar1.Value += emp;
                    }
                    else
                    {
                        progressBar1.Value = 100;
                    }
                }
            }
            else {
                //sum some count and processbar add 1
                tempValue += score;
                Console.WriteLine("tempValue +" + tempValue);
                if (tempValue > 1)
                {
                    score = 1;
                    tempValue = 0;
                }
                else {
                    return progressBar1.Value;
                }
                if (progressBar1.Value <= 100)
                {
                    if (progressBar1.Value + 1 < 100)
                    {                  
                        progressBar1.Value += 1;
                    }
                    else
                    {
                        progressBar1.Value = 100;
                    }
                }
            }
            Console.WriteLine("processbas +" + progressBar1.Value);
            return progressBar1.Value;
          
        }
    }
}
