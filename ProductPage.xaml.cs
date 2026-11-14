using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _41
{
    public partial class ProductPage : Page
    {
        private User currentUser;
        private List<Product> selectedProducts = new List<Product>();
        private List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();

        public ProductPage(User user)
        {
            InitializeComponent();
            currentUser = user;

            if (user != null)
            {
                FIOTB.Text = "Вы авторизованы как " + user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;
                switch (user.UserRole)
                {
                    case 1: RoleTB.Text = "Роль: Клиент"; break;
                    case 2: RoleTB.Text = "Роль: Менеджер"; break;
                    case 3: RoleTB.Text = "Роль: Администратор"; break;
                }
            }
            else
            {
                FIOTB.Text = "";
                RoleTB.Text = "Роль: Гость";
            }

            var currentProducts = Шайдуллин41Entities.GetContext().Product.ToList();
            ProductListView.ItemsSource = currentProducts;
            ComboType.SelectedIndex = 0;
            UpdateProducts();
        }

        private void UpdateProducts()
        {
            var AllProducts = Шайдуллин41Entities.GetContext().Product.ToList();
            var currentProducts = Шайдуллин41Entities.GetContext().Product.ToList();

            if (ComboType.SelectedIndex == 0)
                currentProducts = currentProducts.Where(p => p.ProductDiscountAmount >= 0 && p.ProductDiscountAmount <= 100).ToList();
            if (ComboType.SelectedIndex == 1)
                currentProducts = currentProducts.Where(p => p.ProductDiscountAmount >= 0 && p.ProductDiscountAmount < 9.99).ToList();
            if (ComboType.SelectedIndex == 2)
                currentProducts = currentProducts.Where(p => p.ProductDiscountAmount >= 10 && p.ProductDiscountAmount < 14.99).ToList();
            if (ComboType.SelectedIndex == 3)
                currentProducts = currentProducts.Where(p => p.ProductDiscountAmount >= 15 && p.ProductDiscountAmount <= 100).ToList();

            currentProducts = currentProducts.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            if (RButtonDown.IsChecked.Value)
                currentProducts = currentProducts.OrderByDescending(p => p.ProductCost).ToList();
            if (RButtonUp.IsChecked.Value)
                currentProducts = currentProducts.OrderBy(p => p.ProductCost).ToList();

            TBAllCount.Text = $"Кол-во {currentProducts.Count} из {AllProducts.Count}";
            ProductListView.ItemsSource = currentProducts;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var prod = ProductListView.SelectedItem as Product;
            if (prod == null) return;

            selectedProducts.Add(prod);

            var exists = selectedOrderProducts
                .FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);

            if (exists == null)
            {
                selectedOrderProducts.Add(new OrderProduct
                {
                    ProductArticleNumber = prod.ProductArticleNumber,
                    Quantity = 1,
                    Product = prod
                });
            }
            else
            {
                exists.Quantity++;
            }

            OrderBtn.Visibility = Visibility.Visible;
            ProductListView.SelectedIndex = -1;
        }

        private void ProductListView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(ProductListView, e.OriginalSource as DependencyObject) as ListViewItem;
            if (item != null) item.IsSelected = true;
        }

        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            var w = new OrderWindow(selectedOrderProducts, selectedProducts, currentUser);
            w.Owner = Window.GetWindow(this);
            w.ShowDialog();

            if (w.IsSaved)
            {
                selectedProducts.Clear();
                selectedOrderProducts.Clear();
                OrderBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateProducts();
        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateProducts();
        private void RButtonUp_Checked(object sender, RoutedEventArgs e) => UpdateProducts();
        private void RButtonDown_Checked(object sender, RoutedEventArgs e) => UpdateProducts();

        private void OrderBtn_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}