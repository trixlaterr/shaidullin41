using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _41
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        List<Product> selectedProducts = new List<Product>();
        private User currentUser;
        public bool IsSaved { get; private set; }
        private Order currentOrder = new Order();
        private OrderProduct currentOrderProduct = new OrderProduct();
        public OrderWindow(List<OrderProduct> selectedOrderProducts, List<Product> selectedProducts, User currentuser)
        {
            this.currentUser = currentuser;
            InitializeComponent();
            var currentPickUps = Шайдуллин41Entities.GetContext().PickUpPoint.ToList();
            PickUpPointCB.ItemsSource = currentPickUps;
            PickUpPointCB.SelectedIndex = 0;

            FIOTB.Text = currentUser == null
                ? "Гость"
                : currentUser.UserSurname + " " + currentUser.UserName + " " + currentUser.UserPatronymic;

            var ctx = Шайдуллин41Entities.GetContext();
            OrderIDTB.Text = (ctx.Order.Any() ? ctx.Order.Max(o => o.OrderCode) + 1 : 1).ToString();
            OrderListView.ItemsSource = selectedOrderProducts;
            this.selectedOrderProducts = selectedOrderProducts;
            this.selectedProducts = selectedProducts;
            OrderDP.Text = DateTime.Now.ToString();
            SetDeliveryDate();
            RecalculateTotals();

        }
        private void SetDeliveryDate()
        {
            var articles = selectedProducts.Select(p => p.ProductArticleNumber).ToList();

            var stocks = Шайдуллин41Entities.GetContext().Product
                .Where(p => articles.Contains(p.ProductArticleNumber))
                .Select(p => new { p.ProductArticleNumber, p.ProductQuantityInStock })
                .ToList();

            bool allMoreThan3 = articles.All(a =>
            {
                var s = stocks.FirstOrDefault(x => x.ProductArticleNumber == a);
                return s != null && s.ProductQuantityInStock > 3;
            });

            DeliveryDP.SelectedDate = DateTime.Now.Date.AddDays(allMoreThan3 ? 3 : 6);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOrderProducts.Count == 0)
            {
                MessageBox.Show("Заказ пуст");
                return;
            }

            var pickup = PickUpPointCB.SelectedItem as PickUpPoint;
            if (pickup == null)
            {
                MessageBox.Show("Выберите пункт выдачи");
                return;
            }

            var context = Шайдуллин41Entities.GetContext();

            Order order = new Order();
            order.OrderDate = DateTime.Now;
            order.OrderDeliveryDate = DeliveryDP.SelectedDate ?? DateTime.Now.Date.AddDays(3);
            order.OrderPickupPoint = pickup.PickUpPointID;
            order.OrderUserID = currentUser == null ? (int?)null : currentUser.UserID;
            order.OrderCode = context.Order.Any() ? context.Order.Max(o => o.OrderCode) + 1 : 1;
            order.OrderStatus = "Новый";

            context.Order.Add(order);
            context.SaveChanges();

            foreach (var op in selectedOrderProducts)
            {
                context.OrderProduct.Add(new OrderProduct
                {
                    OrderID = order.OrderID,
                    ProductArticleNumber = op.ProductArticleNumber,
                    Quantity = op.Quantity
                });
            }

            context.SaveChanges();

            IsSaved = true;
            MessageBox.Show("Заказ № " + order.OrderCode + " сохранён");
            Close();
        }
        private void RecalculateTotals()
        {
            decimal total = 0, discount = 0;

            foreach (var op in selectedOrderProducts)
            {
                if (op.Product == null) continue;

                decimal line = op.Quantity * op.Product.ProductCost;
                total += line;

                if (op.Product.ProductDiscountAmount > 0)
                    discount += line * op.Product.ProductDiscountAmount / 100m;
            }

            TotalSumTB.Text = "Сумма: " + total.ToString("N2") + " ₽";
            DiscountTB.Text = "Скидка: " + discount.ToString("N2") + " ₽";
            TotalWithDiscountTB.Text = "Итого: " + (total - discount).ToString("N2") + " ₽";
        }


        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var op = (sender as FrameworkElement)?.DataContext as OrderProduct;
            if (op != null) RemoveOrderProduct(op);
        }

        private void RemoveOrderProduct(OrderProduct op)
        {
            selectedOrderProducts.Remove(op);
            selectedProducts.RemoveAll(p => p.ProductArticleNumber == op.ProductArticleNumber);

            OrderListView.ItemsSource = null;
            OrderListView.ItemsSource = selectedOrderProducts;

            RecalculateTotals();
            SetDeliveryDate();
        }
        private void PlusBtn_Click(object sender, RoutedEventArgs e)
        {
            var op = (sender as FrameworkElement)?.DataContext as OrderProduct;
            if (op == null) return;

            op.Quantity++;
            RefreshOrderList();
        }

        private void MinusBtn_Click(object sender, RoutedEventArgs e)
        {
            var op = (sender as FrameworkElement)?.DataContext as OrderProduct;
            if (op == null) return;

            if (op.Quantity > 1)
            {
                op.Quantity--;
                RefreshOrderList();
            }
            else
            {
                RemoveOrderProduct(op);   // с 1 нажатие «−» = удалить, как «кол-во 0» в методичке
            }
        }

        private void RefreshOrderList()
        {
            OrderListView.ItemsSource = null;
            OrderListView.ItemsSource = selectedOrderProducts;

            RecalculateTotals();
            SetDeliveryDate();
        }
    }
}
