using System.Collections.Generic;
using System.Linq;

namespace AzureMarketplaceFulfillment
{
    public class FulfillmentManagerOperationResult
    {
        private readonly List<FulfillmentManagementError> errors = new List<FulfillmentManagementError>();

        //
        // Summary:
        //     Returns a CustomerUserResult indicating a successful customer user operation.
        //
        // Returns:
        //     An CustomerUserResult indicating a successful customer user operation.
        public static FulfillmentManagerOperationResult Success =>
            new FulfillmentManagerOperationResult { Succeeded = true };

        //
        // Summary:
        //     Flag indicating whether if the operation succeeded or not.

        //
        // Summary:
        //     An System.Collections.Generic.IEnumerable`1 of CustomerUserResultError
        //     containing an errors that occurred during the customer user operation.
        public IEnumerable<FulfillmentManagementError> Errors => errors;

        public bool Succeeded { get; protected set; }
        //
        // Summary:
        //     Creates an CustomerUserResult indicating a failed customer user
        //     operation, with a list of errors if applicable.
        //
        // Parameters:
        //   errors:
        //     An optional array of CustomerUserResultError which caused
        //     the operation to fail.
        //
        // Returns:
        //     An CustomerUserResult indicating a failed customer user
        //     operation, with a list of errors if applicable.

        public static FulfillmentManagerOperationResult Failed(params FulfillmentManagementError[] errors)
        {
            var result = new FulfillmentManagerOperationResult { Succeeded = false };
            if (errors != null)
            {
                result.errors.AddRange(errors);
            }

            return result;
        }

        //
        // Summary:
        //     Converts the value of the current CustomerUserResultError
        //     object to its equivalent string representation.
        //
        // Returns:
        //     A string representation of the current CustomerUserResultError
        //     object.
        //
        // Remarks:
        //     If the operation was successful the ToString() will return "Succeeded" otherwise
        //     it returned "Failed : " followed by a comma delimited list of error codes from
        //     its CustomerUserResultError collection, if any.
        public override string ToString()
        {
            return Succeeded
                ? "Succeeded"
                : string.Format("{0} : {1}", "Failed", string.Join(",", Errors.Select(x => x.Code).ToList()));
        }
    }
}