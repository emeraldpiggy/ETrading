namespace ETrading.Framework.GCNotifier
{
    public interface IGcNotifierRegistration
    {
        /// <summary>
        /// On collection Events 
        /// </summary>
        void OnCollection();


        /// <summary>
        /// Gets or sets the slot that is used in the GCNotifierBuffer not the slot index is +1.
        /// </summary>
        int Slot { get; set; }


        /// <summary>
        /// The slot references
        /// </summary>
        int SlotReferecnes { get; set; }
    }
}
