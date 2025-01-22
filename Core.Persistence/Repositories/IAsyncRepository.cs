using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Persistence.Repositories;

public interface IAsyncRepository<TEntity, TEntityId>: IQuery<TEntity>
    where TEntity : Entity<TEntityId>
{
    /// <summary>
    /// Belirtilen kritere göre asenkron olarak bir varlığı getirir.
    /// İlişkili varlıkları dahil edebilir, isteğe bağlı olarak  silinmiş (soft delete) kayıtları ve değişiklik izleme (tracking) özelliğini kontrol etme seçenekleri sunar.
    /// Ayrıca uzun süren işlemler için iptal desteği sağlar.
    /// </summary>
    /// <param name="predicate">Varlığın sorgulanacağı kriter.</param>
    /// <param name="include">Opsiyonel. İlişkili varlıkları dahil etmek için kullanılan fonksiyon.</param>
    /// <param name="withDeleted">Opsiyonel. soft delete  kayıtların da dahil edilmesi için true olarak ayarlanabilir.</param>
    /// <param name="enableTracking">Opsiyonel. Değişikliklerin izlenmesi için takip özelliğini etkinleştirmek istiyorsanız true olarak ayarlayın. Performans için false olabilir.</param>
    /// <param name="cancellationToken">Opsiyonel. Asenkron işlemi iptal etmek için kullanılan token.</param>
    /// <returns>Bulunan varlık ya da eşleşme bulunamazsa null döner.</returns>
    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );


    Task<Paginate<TEntity>> GetListAsync(
      Expression<Func<TEntity, bool>>? predicate = null,
      Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
      Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
      int index = 0,
      int size = 10,
      bool withDeleted = false,
      bool enableTracking = true,
      CancellationToken cancellationToken = default
  );


    Task<Paginate<TEntity>> GetListByDynamicAsync(
          DynamicQuery dynamic,
          Expression<Func<TEntity, bool>>? predicate = null,
          Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
          int index = 0,
          int size = 10,
          bool withDeleted = false,
          bool enableTracking = true,
          CancellationToken cancellationToken = default
      );
    Task<bool> AnyAsync(
       Expression<Func<TEntity, bool>>? predicate = null,
       bool withDeleted = false,
       bool enableTracking = true,
       CancellationToken cancellationToken = default
   );


    Task<TEntity> AddAsync(TEntity entity);

    Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities);

    Task<TEntity> UpdateAsync(TEntity entity);

    Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities);

    Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false);

    Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false);
}
