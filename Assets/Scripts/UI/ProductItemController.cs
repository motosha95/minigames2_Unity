using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minigames.Data;

namespace Minigames.UI
{
    /// <summary>
    /// Controller for individual product item in the marketplace.
    /// </summary>
    public class ProductItemController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI productNameText;
        [SerializeField] private TextMeshProUGUI productDescriptionText;
        [SerializeField] private TextMeshProUGUI productCostText;
        [SerializeField] private Image productImage;
        [SerializeField] private Button selectButton;

        private Product productInfo;
        private System.Action<Product> onProductSelected;

        private void Start()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(OnSelectButtonClicked);
            }
        }

        /// <summary>
        /// Setup the product item with product info
        /// </summary>
        public void Setup(Product product, System.Action<Product> onSelected)
        {
            productInfo = product;
            onProductSelected = onSelected;

            if (productNameText != null)
                productNameText.text = product.name ?? "Unknown Product";

            if (productDescriptionText != null)
                productDescriptionText.text = product.description ?? "";

            if (productCostText != null)
                productCostText.text = $"{product.cost} {product.currencyType}";

            // TODO: Load product image from URL if needed
            // if (productImage != null && !string.IsNullOrEmpty(product.imageUrl))
            // {
            //     StartCoroutine(LoadProductImage(product.imageUrl));
            // }
        }

        private void OnSelectButtonClicked()
        {
            if (productInfo != null && onProductSelected != null)
            {
                onProductSelected.Invoke(productInfo);
            }
        }
    }
}
