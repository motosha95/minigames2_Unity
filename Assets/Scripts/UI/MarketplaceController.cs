using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Minigames.Data;
using Minigames.Managers;

namespace Minigames.UI
{
    /// <summary>
    /// Controls the marketplace UI panel.
    /// Displays available products from the API.
    /// </summary>
    public class MarketplaceController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform productsListContainer;
        [SerializeField] private GameObject productItemPrefab;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Text loadingText;
        [SerializeField] private GameObject emptyStatePanel;

        private List<Product> availableProducts = new List<Product>();

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            LoadProducts();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeUI()
        {
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(LoadProducts);
            }
        }

        private void SubscribeToEvents()
        {
            ProductManager.Instance.OnProductsLoaded += HandleProductsLoaded;
            ProductManager.Instance.OnProductsLoadError += HandleProductsLoadError;
        }

        private void UnsubscribeFromEvents()
        {
            if (ProductManager.Instance != null)
            {
                ProductManager.Instance.OnProductsLoaded -= HandleProductsLoaded;
                ProductManager.Instance.OnProductsLoadError -= HandleProductsLoadError;
            }
        }

        private void LoadProducts()
        {
            ShowLoading(true);
            ProductManager.Instance.LoadProducts();
        }

        private void ShowLoading(bool show)
        {
            if (loadingText != null)
                loadingText.gameObject.SetActive(show);
        }

        private void HandleProductsLoaded(List<Product> products)
        {
            availableProducts = products;
            ShowLoading(false);
            PopulateProductsList();
        }

        private void HandleProductsLoadError(string error)
        {
            ShowLoading(false);
            PopupManager.Instance.ShowError("Marketplace Error", $"Failed to load products: {error}");
        }

        private void PopulateProductsList()
        {
            if (productsListContainer == null || productItemPrefab == null)
                return;

            // Clear existing items
            foreach (Transform child in productsListContainer)
            {
                Destroy(child.gameObject);
            }

            // Show empty state if no products
            if (availableProducts.Count == 0)
            {
                if (emptyStatePanel != null)
                    emptyStatePanel.SetActive(true);
                return;
            }

            if (emptyStatePanel != null)
                emptyStatePanel.SetActive(false);

            // Create items for each product
            foreach (var product in availableProducts)
            {
                GameObject itemObj = Instantiate(productItemPrefab, productsListContainer);
                ProductItemController itemController = itemObj.GetComponent<ProductItemController>();
                
                if (itemController != null)
                {
                    itemController.Setup(product, OnProductSelected);
                }
            }
        }

        private void OnProductSelected(Product product)
        {
            Debug.Log($"MarketplaceController: Selected product {product.name} (ID: {product.id})");
            // Note: Product redemption is handled by host app, not Unity
            // Just show product details or notify host app
            PopupManager.Instance.ShowMessage(
                product.name,
                $"{product.description}\n\nCost: {product.cost} {product.currencyType}"
            );
        }
    }
}
