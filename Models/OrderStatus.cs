namespace GarageTrack.Models;

public enum OrderStatus
{
    Received,          // Réceptionné
    QuoteInProgress,   // Devis en préparation
    QuotePending,      // Devis à valider (liste atelier)
    QuoteToSign,       // À valider (aperçu du devis)
    InRepair,          // En réparation
    MissingPart,       // Pièce manquante
    Done               // Terminé
}
