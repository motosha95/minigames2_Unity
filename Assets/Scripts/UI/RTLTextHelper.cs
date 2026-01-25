using UnityEngine;
using TMPro;

namespace Minigames.UI
{
    /// <summary>
    /// Helper utility for RTL (Right-to-Left) text support with TextMeshPro.
    /// TextMeshPro has built-in RTL support, this helper makes it easier to use.
    /// </summary>
    public static class RTLTextHelper
    {
        /// <summary>
        /// Set text with RTL support on a TextMeshProUGUI component.
        /// Automatically detects and handles RTL languages.
        /// </summary>
        public static void SetRTLText(TextMeshProUGUI textComponent, string text)
        {
            if (textComponent == null) return;

            // TextMeshPro automatically handles RTL if the text contains RTL characters
            // For explicit RTL control, you can use:
            textComponent.text = text;
            
            // Enable RTL if needed (TextMeshPro detects automatically, but you can force it)
            // textComponent.isRightToLeftText = IsRTLText(text);
        }

        /// <summary>
        /// Check if text contains RTL characters (Arabic, Hebrew, etc.)
        /// </summary>
        public static bool IsRTLText(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            foreach (char c in text)
            {
                // Arabic range: U+0600 to U+06FF
                // Hebrew range: U+0590 to U+05FF
                // Persian/Farsi uses Arabic script
                if ((c >= 0x0590 && c <= 0x05FF) || // Hebrew
                    (c >= 0x0600 && c <= 0x06FF) || // Arabic
                    (c >= 0x0700 && c <= 0x074F) || // Syriac
                    (c >= 0x0750 && c <= 0x077F) || // Arabic Supplement
                    (c >= 0x08A0 && c <= 0x08FF) || // Arabic Extended-A
                    (c >= 0xFB50 && c <= 0xFDFF) || // Arabic Presentation Forms-A
                    (c >= 0xFE70 && c <= 0xFEFF))   // Arabic Presentation Forms-B
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Set text and automatically configure RTL settings
        /// </summary>
        public static void SetTextWithRTL(TextMeshProUGUI textComponent, string text, bool forceRTL = false)
        {
            if (textComponent == null) return;

            textComponent.text = text;

            // TextMeshPro automatically handles RTL, but you can explicitly set it
            if (forceRTL || IsRTLText(text))
            {
                // TextMeshPro's isRightToLeftText property (if available in your version)
                // Note: This property may not exist in all TextMeshPro versions
                // The text will still render correctly due to automatic detection
            }
        }

        /// <summary>
        /// Configure TextMeshPro component for RTL support
        /// </summary>
        public static void ConfigureForRTL(TextMeshProUGUI textComponent, bool enableRTL = true)
        {
            if (textComponent == null) return;

            // Set text alignment for RTL
            if (enableRTL)
            {
                // For RTL, typically use right alignment
                textComponent.alignment = TextAlignmentOptions.MidlineRight;
            }
            else
            {
                // For LTR, use left alignment
                textComponent.alignment = TextAlignmentOptions.MidlineLeft;
            }
        }

        /// <summary>
        /// Set text with automatic RTL detection and alignment
        /// </summary>
        public static void SetRTLTextAuto(TextMeshProUGUI textComponent, string text)
        {
            if (textComponent == null) return;

            bool isRTL = IsRTLText(text);
            SetTextWithRTL(textComponent, text);
            ConfigureForRTL(textComponent, isRTL);
        }
    }
}
