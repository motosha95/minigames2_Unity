using System;
using System.Collections.Generic;
using UnityEngine;
using Minigames.Core;
using Minigames.Data;

namespace Minigames.Managers
{
    /// <summary>
    /// Manages marketplace products data.
    /// Handles product list retrieval and caching.
    /// </summary>
    public class ProductManager : MonoBehaviour
    {
        private static ProductManager _instance;
        public static ProductManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ProductManager");
                    _instance = go.AddComponent<ProductManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Events
        public event Action<List<Product>> OnProductsLoaded;
        public event Action<string> OnProductsLoadError;

        private List<Product> availableProducts = new List<Product>();
        private Dictionary<string, Product> productCache = new Dictionary<string, Product>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Load products from API
        /// </summary>
        public void LoadProducts(int page = 1, int pageSize = 20, Action<List<Product>> onSuccess = null, Action<string> onError = null)
        {
            string endpoint = $"/api/App/products?page={page}&pageSize={pageSize}";
            ApiClient.Instance.Get<ProductListResponse>(
                endpoint,
                (response) =>
                {
                    availableProducts = response.data.products ?? new List<Product>();
                    productCache.Clear();
                    
                    foreach (var product in availableProducts)
                    {
                        productCache[product.id] = product;
                    }

                    OnProductsLoaded?.Invoke(availableProducts);
                    onSuccess?.Invoke(availableProducts);
                },
                (error) =>
                {
                    OnProductsLoadError?.Invoke(error);
                    onError?.Invoke(error);
                }
            );
        }

        /// <summary>
        /// Get all available products (from cache)
        /// </summary>
        public List<Product> GetProducts()
        {
            return new List<Product>(availableProducts);
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        public Product GetProductById(string productId)
        {
            return productCache.ContainsKey(productId) ? productCache[productId] : null;
        }

        /// <summary>
        /// Check if products are loaded
        /// </summary>
        public bool AreProductsLoaded()
        {
            return availableProducts.Count > 0;
        }
    }
}
