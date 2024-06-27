using Microsoft.OpenApi.Attributes;
using System.Runtime.Serialization;

namespace MagicLand_System.Enums
{
    public enum TransactionTypeEnum
    {
        [Display("TopUp")]
        TopUp,
        Refund,
        Payment,
    }
}
