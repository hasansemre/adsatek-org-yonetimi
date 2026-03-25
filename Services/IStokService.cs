using OrgYonetimi.Models;
using OrgYonetimi.ViewModels;

namespace OrgYonetimi.Services;

public interface IStokService
{
    Task<List<StokKarti>> GetAllAsync();
    Task<StokKarti?> GetByIdAsync(int id);
    Task<StokKarti?> GetByIdWithTedariklerAsync(int id);
    Task<StokKarti> CreateAsync(StokKarti stokKarti);
    Task UpdateAsync(StokKarti stokKarti);
    Task DeleteAsync(int id);
    Task<decimal> GetToplamStokAsync(int stokKartiId);
    Task<List<FirmaStokOzet>> GetFirmaBazliStokAsync(int stokKartiId);
    Task<decimal> GetFirmaStokAsync(int stokKartiId, int temsilciFirmaId);
    Task<List<StokHareket>> GetHareketlerAsync(int stokKartiId);
    Task StokGirisiYapAsync(int stokKartiId, int? temsilciFirmaId, decimal miktar, int? tedarikId, string aciklama, string olusturanId);
    Task<bool> StokCikisiYapAsync(int stokKartiId, int? temsilciFirmaId, decimal miktar, int? gorevItemId, string aciklama, string olusturanId);
    Task<List<StokKarti>> GetStokluUrunlerAsync();
}
