using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace algLab4
{
    public partial class Form1 : Form
    {
        StorService storage;
        Graphics paintForm;
        char a;
        public Form1()
        {
            InitializeComponent();

            paintForm = CreateGraphics();
            storage = new StorService();
            a = (char)65;
        }

        public class Edge
        {
            private Point p1;
            private Point p2;
            
            private Pen defaultPen = new Pen(Color.Black, 2);
            private Pen spanningPen = new Pen(Color.Red, 6);        // для остовного дерева

            private bool is_spanning = false;
            private List<Ver> vers;

            public void make_spanning()
            {
                is_spanning = true;
            }
            public void make_default()
            {
                is_spanning = false;
            }
            public bool is_Connected(Ver ver)
            {
                return vers.Contains(ver);
            }
            public bool are_Connected(Ver ver1, Ver ver2)
            {
                bool a = vers.Contains(ver1);
                bool b = vers.Contains(ver2);
                return (a && b);
            }
            
            public void paint(Graphics paintForm)
            {
                if (is_spanning == false)
                    paintForm.DrawLine(defaultPen, p1, p2);
                else
                    paintForm.DrawLine(spanningPen, p1, p2);
            }
            public Edge(Point p1, Point p2, Ver ver1, Ver ver2, Graphics paintForm)
            {
                this.p1 = p1;
                this.p2 = p2;

                p1.X += ver1.r;
                p1.Y += ver1.r;
                p2.X += ver2.r;
                p2.Y += ver2.r;

                vers = new List<Ver>() { ver1, ver2 };
                paint(paintForm);
            }
        }

        public class Ver
        {
            private Rectangle rect;
            private int x;
            private int y;
            public int r = 30;
            private bool is_focused;

            public string name;
            private int name_x;
            private int name_y;
            private int name_size;

            Pen defaultPen = new Pen(Color.Black, 4);
            Pen focusedPen = new Pen(Color.Blue, 4);
            SolidBrush defaultBrush = new SolidBrush(Color.White);
            SolidBrush focusedBrush = new SolidBrush(Color.LightSkyBlue);

            public List<Edge> edges = new List<Edge>();
            public List<Ver> neighbours = new List<Ver>();

            public bool is_spanned = false;

            //drawing method
            public void paint(Graphics paintForm)
            {
                if (is_focused == true)
                {
                    paintForm.DrawEllipse(focusedPen, rect);
                    paintForm.FillEllipse(focusedBrush, rect);
                }
                else
                {
                    paintForm.DrawEllipse(defaultPen, rect);
                    paintForm.FillEllipse(defaultBrush, rect);
                }

                paintForm.DrawString(name, new Font("Arial", name_size), new SolidBrush(Color.Black), name_x, name_y);
            }
            public Point getPos()
            {
                return new Point(x + r + ((int)(focusedPen.Width / 2)), y + r + ((int)(focusedPen.Width / 2)));
            }

            public void edgeRemove(Edge edge)
            {
                edges.Remove(edge);
            }
            public void neighbourRemove(Ver ver, List<Edge> edges)
            {
                foreach (Edge edge in this.edges)
                {
                    if (edge.is_Connected(ver))
                    {
                        edgeRemove(edge);
                        edges.Remove(edge);
                        break;
                    }
                }
                neighbours.Remove(ver);
            }

            // focus
            public bool focusCheck()
            {
                if (is_focused == true)
                    return true;
                else
                    return false;
            }
            public void focus(Graphics paintForm)
            {
                is_focused = true;
                paint(paintForm);
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

            public void addEdge(Edge edge)
            {
                edges.Add(edge);
            }
            public void addNeighbour(Ver ver)
            {
                neighbours.Add(ver);
            }

            public bool neighbourCheck(Ver ver)
            {
                return neighbours.Contains(ver);
            }
            public void span(Ver ver, Graphics paintForm)
            {
                foreach (Edge edge in edges)
                {
                    if (edge.are_Connected(this, ver) && ver.is_spanned == false)
                    {
                        edge.make_spanning();
                        ver.is_spanned = true;
                    }
                }
            }

            public Ver(int x, int y, Graphics paintForm, char a)
            {
                this.x = x - r - ((int)(focusedPen.Width / 2));
                this.y = y - r - ((int)(focusedPen.Width / 2));
                is_focused = true;
                rect = new Rectangle(this.x, this.y, r * 2, r * 2);

                name = a.ToString();
                name_size = 14;
                name_x = x - name_size + ((int)(focusedPen.Width / 2));
                name_y = y - name_size;

                paint(paintForm);
                
            }
        }

        public class MyStorage
        {
            protected Ver[] storage;
            protected List<Edge> edges = new List<Edge>();
            protected int iter;
            protected int size;
            protected int count;

            public void remove()                            // removes all nulled elements
            {
                int del = 0;

                for (int i = 0; i < size; i++)
                    if (storage[i] != null)
                        del = del + 1;

                Ver[] tempStorage = new Ver[del];   // here we'll put elements that should remain

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

                storage = new Ver[size];
                for (int i = 0; i < size; i++)
                    storage[i] = tempStorage[i];            // moved all remained elements
            }
            private void sizeImprove()
            {
                Ver[] tempStorage = storage;
                size = size + 1;
                storage = new Ver[size];

                for (int i = 0; i < size - 1; i++)
                    storage[i] = tempStorage[i];
                storage[size - 1] = null;

            }

            public void add(Ver circle, Graphics ellipses)
            {
                if (count != 0)
                    foreach (Ver c in storage)
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
                storage = new Ver[size];
            }
        }

        public class StorService : MyStorage
        {
            public void removeFocused(Graphics paintForm)
            {
                for (int i = 0; i < size; i++)
                {
                    if (storage[i].focusCheck() == true)
                    {
                        foreach (Ver ver in storage)
                            ver.neighbourRemove(storage[i], edges);


                        storage[i] = null;                  // placing null in the storage at the elements we should delete
                    }
                }

                remove();
                //now at the form's paint event we won't draw elements those were focused. Let's make the form repaint it immediately.
                ActiveForm.Invalidate();
            }

            public void focusOnClick(Graphics paintForm, int x_mouse, int y_mouse)
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
                    storage[i].focus(paintForm);             // выделили вершину, на которую нажали

                    int j = 0;
                    found = false;                  // проверка, есть ли ещё выделенные вершины
                    while (found == false && j < size)
                    {
                        if (i != j && storage[j].focusCheck() == true)
                        {
                            found = true;
                            continue;
                        }
                        j++;
                    }

                    if (j < size)
                    {                               // нужно создать ребро

                        storage[i].unfocus();
                        storage[j].unfocus();

                        foreach (Edge e in edges)
                        {
                            if (e.are_Connected(storage[i], storage[j]))
                                return;
                        }

                        Edge edge = new Edge(storage[i].getPos(), storage[j].getPos(), storage[i], storage[j], paintForm);
                        storage[i].addEdge(edge);
                        storage[j].addEdge(edge);
                        edges.Add(edge);

                        storage[i].addNeighbour(storage[j]);
                        storage[j].addNeighbour(storage[i]);

                    }
                }
                else
                    foreach (Ver c in storage)
                        c.unfocus();
            }
            public void paint(Graphics paintForm)
            {
                foreach (Edge e in edges)
                    e.paint(paintForm);

                if (count != 0)
                    foreach (Ver circle in storage)
                        circle.paint(paintForm);
            }

            public void paintSpan(List<Ver> L, Graphics paintForm)
            {
                for (int i = 0; i < L.Count - 1; i++)
                    for (int j = i + 1; j < L.Count; j++)
                        if (L[i].neighbourCheck(L[j]) == true)
                        {
                            L[i].span(L[j], paintForm);
                            paint(paintForm);
                            Thread.Sleep(500);
                        }
                        else
                        {
                            L[i].is_spanned = true;
                            continue;
                        }
            }

            public void inWidth(Ver ver, List<Ver> Och, List<Ver> L)
            {
                Och.Remove(ver);

                if (L.Contains(ver) == false)
                {
                    L.Add(ver);
                }
                else
                    return;

                foreach (Ver v in ver.neighbours)
                {
                    if (L.Contains(v) == false)
                        Och.Add(v);
                }


            }
            public string inWidthPrep(Graphics paintForm)
            {
                if (count == 0)
                    return "";

                List <Ver> Och = new List<Ver>();
                List<Ver> L = new List<Ver>();

                bool found = false;
                for (int i = 0; i < size; i++)
                    if (storage[i].focusCheck() == true)
                    {
                        inWidth(storage[i], Och, L);
                        while (Och.Count != 0)
                            inWidth(Och[0], Och, L);

                        found = true;
                        break;
                    }

                if (found == false)
                {
                    inWidth(storage[0], Och, L);
                    while (Och.Count != 0)
                        inWidth(Och[0], Och, L);
                }
                if (L.Count != count)
                {
                    MessageBox.Show("Граф несвязный.");
                    return "нет";
                    // расписать обход какого-либо графа в ширину
                    // представить граф в виде матрицы инцеденций
                    // недостатки матрицы инцеденций
                }
                else
                {
                    paintSpan(L, paintForm);

                    foreach (Ver v in storage)
                        v.is_spanned = false;

                    foreach (Edge e in edges)
                        e.make_default();

                    string res = "";
                    foreach (Ver v in L)
                        res += v.name;

                    return res;
                }
            }
        }
        ///////// ended up for the storages and Ver classes



        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            paintForm = CreateGraphics();
            storage.paint(paintForm);
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            //PointToClient returns mouse position in relation to the form, not to the screen
            Point mousePos = PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));
            storage.add(new Ver(mousePos.X, mousePos.Y, paintForm, a), paintForm);
            a++;
        }


        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));
                storage.focusOnClick(paintForm, mousePos.X, mousePos.Y);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            storage.removeFocused(paintForm);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string res = storage.inWidthPrep(paintForm);
            if (res != "")
                label1.Text = "Путь: " + res;
            else
                label1.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            storage.remove();
            storage = new StorService();
            a = (char)65;
            ActiveForm.Invalidate();
        }
    }
}
