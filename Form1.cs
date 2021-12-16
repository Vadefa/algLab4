using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace algLab4
{
    public partial class Form1 : Form
    {
        StorService storage;
        Graphics paintForm;
        bool ctrl;
        public Form1()
        {
            InitializeComponent();

            paintForm = CreateGraphics();
            storage = new StorService();

        }

        public class CCircle
        {
            private Rectangle rect;
            private int x;
            private int y;
            private int r = 30;
            private bool is_focused;

            Pen defaultPen = new Pen(Color.Black, 4);
            Pen focusedPen = new Pen(Color.Blue, 4);


            //drawing method
            public void paint(Graphics paintForm)
            {
                if (this.is_focused == true)
                    paintForm.DrawEllipse(focusedPen, rect);
                else
                    paintForm.DrawEllipse(defaultPen, rect);
            }


            // focus
            public bool focusCheck()
            {
                if (is_focused == true)
                    return true;
                else
                    return false;
            }
            public void focus()
            {
                is_focused = true;
                ActiveForm.Invalidate();
            }
            public void unfocus()
            {
                is_focused = false;
                ActiveForm.Invalidate();
            }


            // something with coordinates
            public bool checkUnderMouse(Graphics paintForm, int x_mouse, int y_mouse)
            {
                int x0 = x;
                int y0 = y;

                int x1 = x + r * 2 + ((int)(defaultPen.Width / 2));
                int y1 = y + r * 2 + ((int)(defaultPen.Width / 2));

                if ((x_mouse >= x0) && (x_mouse <= x1) && (y_mouse >= y0) && (y_mouse <= y1))
                    return true;
                else
                    return false;
            }

            public CCircle(int x, int y, Graphics paintForm)
            {
                this.x = x - r - ((int)(focusedPen.Width / 2));
                this.y = y - r - ((int)(focusedPen.Width / 2));
                is_focused = true;
                rect = new Rectangle(this.x, this.y, r * 2, r * 2);

                paint(paintForm);
            }
        }



        public class MyStorage
        {
            protected CCircle[] storage;
            protected int iter;
            protected int size;
            protected int count;

            public void remove()                            // removes all nulled elements
            {
                int del = 0;

                for (int i = 0; i < size; i++)
                    if (storage[i] != null)
                        del = del + 1;


                CCircle[] tempStorage = new CCircle[del];   // here we'll put elements that should remain

                int j = 0;
                for (int i = 0; i < size; i++)
                    if (storage[i] != null)
                    {
                        tempStorage[j] = storage[i];        // putting remaining elements
                        j = j + 1;
                    }

                size = del;                                 // changing properties
                count = size;
                iter = size;
                if (iter < 0)
                    iter = 0;


                storage = new CCircle[size];
                for (int i = 0; i < size; i++)
                    storage[i] = tempStorage[i];            // moved all remained elements
            }
            private void sizeImprove()
            {
                CCircle[] tempStorage = storage;


                size = size + 1;

                storage = new CCircle[size];

                for (int i = 0; i < size - 1; i++)
                    storage[i] = tempStorage[i];

                storage[size - 1] = null;

            }

            public void add(CCircle circle, Graphics ellipses)
            {
                if (count != 0)
                    foreach (CCircle c in storage)
                        c.unfocus();

                if (iter < size)
                {
                    if (storage[iter] == null)
                    {
                        storage[iter] = circle;
                        iter = iter + 1;
                    }
                }
                else if (iter == size)
                {
                    sizeImprove();
                    storage[iter] = circle;
                    iter = iter + 1;
                }
                count = count + 1;
            }
            public MyStorage()
            {
                iter = 0;
                count = 0;
                size = 1;
                storage = new CCircle[size];
            }
        }

        public class StorService : MyStorage
        {
            public void removeFocused(Graphics paintForm)
            {
                for (int i = 0; i < size; i++)
                {
                    if (storage[i].focusCheck() == true)
                        storage[i] = null;                  // placing null in the storage at the elements we should delete
                }

                remove();

                //now at the form's paint event we won't draw elements those were focused. Let's make the form repaint it immediately.
                ActiveForm.Invalidate();
            }


            public void focusOnClick(Graphics paintForm, int x_mouse, int y_mouse, bool ctrl)
            {
                if (count == 0)
                    return;


                int i = size;
                bool found = false;
                while ((found == false) && (i > 0))
                {
                    i = i - 1;
                    found = storage[i].checkUnderMouse(paintForm, x_mouse, y_mouse);
                }


                if (found == true)
                {
                    if (ctrl == false)
                        foreach (CCircle circle in storage)
                            circle.unfocus();

                    storage[i].focus();
                }

            }

            public void paint(Graphics paintForm)
            {
                if (count != 0)
                    foreach (CCircle circle in storage)
                        circle.paint(paintForm);
            }
        }
        // ended up for the storages and ccircle classes



        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            paintForm = CreateGraphics();

            storage.paint(paintForm);
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            //PointToClient returns mouse position in relation to the form, not to the screen
            Point mousePos = PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));
            storage.add(new CCircle(mousePos.X, mousePos.Y, paintForm), paintForm);
        }


        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));
                storage.focusOnClick(paintForm, mousePos.X, mousePos.Y, ctrl);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                storage.removeFocused(paintForm);

            if (e.KeyCode == Keys.ControlKey)
                ctrl = true;

        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
                ctrl = false;
        }
    }
}
