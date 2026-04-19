using System.ComponentModel.DataAnnotations.Schema;
using Base.Enum;

namespace Base.Entities;

[Table("UserBranchTransfer", Schema = "Base")]
public class UserBranchTransfer : BaseEntity
    {
        #region Request Info

        public virtual User User { get; set; }
        public long UserId { get; set; }

        public virtual Branch FromBranch { get; set; }

        public long FromBranchId { get; set; }

        public virtual Branch ToBranch { get; set; }
        public long ToBranchId { get; set; }

        public virtual User RequestInitiator { get; set; }
        public long RequestInitiatorId { get; set; }
        public string RequestNote { get; set; }
        public DateTime RequestSentOn { get; set; }

        #endregion

        #region Response Info

        /// <summary>
        /// This will be null when the response has not been made yet
        /// </summary>
        public virtual User? Responder { get; set; }

        public long? ResponderId { get; set; }
        public string? ResponseNote { get; set; }
        public DateTime? ResponseOn { get; set; }

        #endregion

        public UserBranchTransferStatus TransferStatus { get; set; } = UserBranchTransferStatus.Requested;

        public bool IsRequested() => TransferStatus == UserBranchTransferStatus.Requested;
        public bool IsAccepted() => TransferStatus == UserBranchTransferStatus.Approved;
        public bool IsRejected() => TransferStatus == UserBranchTransferStatus.Rejected;
        public bool IsCancelled() => TransferStatus == UserBranchTransferStatus.Cancelled;

        protected UserBranchTransfer()
        {
        }

        public UserBranchTransfer(User user, Branch fromBranch, Branch toBranch, string requestNote,
            User initiator)
        {
            User = user;
            FromBranch = fromBranch;
            ToBranch = toBranch;
            RequestNote = requestNote;
            RequestInitiator = initiator;
            RequestSentOn = DateTime.Now;
        }

        public void Approve(User responder, string responseNote)
            => SetResponseStatus(UserBranchTransferStatus.Approved, responder, responseNote);

        public void Reject(User responder, string responseNote)
            => SetResponseStatus(UserBranchTransferStatus.Rejected, responder, responseNote);

        public void CancelRequest(User responder, string internalNote)
            => SetResponseStatus(UserBranchTransferStatus.Cancelled, responder, internalNote);

        private void SetResponseStatus(UserBranchTransferStatus transferStatus, User responder,
            string responseNote)
        {
            TransferStatus = transferStatus;
            Responder = responder;
            ResponseNote = responseNote;
            ResponseOn = DateTime.Now;
        }
    }