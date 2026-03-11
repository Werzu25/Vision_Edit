using Models;
using Microsoft.EntityFrameworkCore;

namespace ORM.Services
{
    internal class DocumentService
    {
        private readonly DbManager _dbManager;

        public DocumentService(DbManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<List<DocumentModel>> GetAllDocuments()
        {
            return await _dbManager.Documents.ToListAsync();
        }

        public async Task<DocumentModel?> GetDocumentByNameForUser(string name, int userId)
        {
            return await _dbManager.Documents
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Name == name && d.User.Id == userId);
        }

        public async Task<DocumentModel?> CreateDocument(DocumentModel documentModel)
        {
            var createdDocument = (await _dbManager.AddAsync(documentModel)).Entity;
            if (await _dbManager.SaveChangesAsync() > 0)
            {
                return createdDocument;
            }

            return null;
        }

        public async Task<DocumentModel?> UpdateDocument(DocumentModel documentModel)
        {
            _dbManager.Documents.Update(documentModel);
            if (await _dbManager.SaveChangesAsync() > 0)
            {
                return documentModel;
            }

            return null;
        }
    }
}
