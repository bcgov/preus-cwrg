using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public interface IEvaluationNoteService
	{
		GrantApplicationEvaluationNote Get(int id);
		GrantApplicationEvaluationNote Add(GrantApplicationEvaluationNote note);
		GrantApplicationEvaluationNote Update(GrantApplicationEvaluationNote note);
		void Remove(GrantApplicationEvaluationNote note);
		GrantApplicationEvaluationNote CreateNote(GrantApplication grantApplication, NoteTypes noteType, string message, Attachment attachment = null);
	}
}