using CityworksOfficeServiceApp.Services;
using CPW_Cityworks.Abstractions;
using CPW_Contact;
using XTI_App.Abstractions;

namespace CityworksOfficeServiceAppTests;

public sealed class FakeCityworksService : ICityworksService
{
    private static int caseID = 1;
    private static int caseTaskID = 312;
    private static int caseFeeID = 1119;
    private static int casePaymentID = 1122;
    private static int dataGroupID = 719;
    private static int dataGroupDetailID = 1920;
    private static int receiptID = 1233;
    private readonly List<CwTenderTypeModel> tenderTypes = new();
    private readonly List<CaseDetailModel> caseDetails = new();
    private readonly List<CustomFieldCategoryModel> customFieldCategories = new();
    private readonly List<CaseReceiptDetailModel> receiptDetails = new();

    public FakeCityworksService()
    {
        tenderTypes.Add(new CwTenderTypeModel(1, CwTenderCode.Values.Cash, "Cash"));
        tenderTypes.Add(new CwTenderTypeModel(2, CwTenderCode.Values.Check, "Check"));
        tenderTypes.Add(new CwTenderTypeModel(3, CwTenderCode.Values.CreditCard, "Credit Card"));
    }

    public Task<CwTenderTypeModel[]> GetTenderTypes(CancellationToken ct) => Task.FromResult(tenderTypes.ToArray());

    public CaseDetailModel AddInstallServiceCase
    (
        string location,
        string accountType,
        string companyName,
        Address billingAddress,
        CwCoordinates coords
    )
    {
        var caseDetail = new CaseDetailModel
        (
            Case: new CaseModel
            (
                ID: caseID,
                OfficeUrl: "",
                RespondUrl: "",
                BusinessCaseID: 0,
                CaseType: new CaseTypeModel(),
                SubTypeID: 0,
                SubTypeDescription: "",
                TimeCreated: DateTimeOffset.Now,
                TimeExpired: DateTimeOffset.MaxValue,
                CaseStatus: new CaseStatusModel
                (
                    ID: 0,
                    Code: CaseStatusCode.Values.Open,
                    Value: "OPEN"
                ),
                CaseName: "Install Service",
                CaseNumber: $"SVC-{caseID:0000}",
                BusinessName: new PllBusinessName(),
                Location: location,
                Coordinates: coords
            ),
            EnteredBy: new CwUserModel(),
            DataGroupDetails: [],
            FeeDetails: [],
            NonFeePayments: [],
            People:
            [
                new CasePersonModel
                (
                    ID: 0,
                    Person: new CwPersonModel
                    (
                        ID: 0,
                        CompanyName: companyName,
                        PersonName: "",
                        HomePhone: new TelephoneNumber(),
                        WorkPhone: new TelephoneNumber(),
                        CellPhone: new TelephoneNumber(),
                        Email: "",
                        Address: billingAddress,
                        Comment: new()
                    ),
                    Roles:
                    [
                        new CasePersonRoleModel
                        (
                            ID: 0,
                            Role: new CaseRoleModel
                            (
                                ID: 0,
                                Code: "DEVELOPER",
                                Description: "",
                                RequiredFields: []
                            )
                        )
                    ]
                )
            ],
            TaskDetails: [],
            MapLayers: [],
            RelatedDocuments: [],
            RelatedActivities: new CwActivityRelationship(),
            Receipts: []
        );
        caseID++;
        caseDetails.Add(caseDetail);
        caseDetail = SetCaseData(caseDetail, "CUSTOMER", "COMM_RES", accountType.ToUpper());
        caseDetail = SetCaseData(caseDetail, "CUSTOMER", "CUS DEV NO", "");
        caseDetail = SetCaseData(caseDetail, "CITY LIMIT", "IN OR OUT", "");
        caseDetail = SetCaseData(caseDetail, "INTERNAL", "SVC LOCATE", "");
        return caseDetail;
    }

    public CaseDetailModel LocationIsInsideTheCity(CaseDetailModel caseDetail) =>
        SetCaseData(caseDetail, "CITY LIMIT", "IN OR OUT", "Inside");

    public CaseDetailModel LocationIsOutsideTheCity(CaseDetailModel caseDetail) =>
        SetCaseData(caseDetail, "CITY LIMIT", "IN OR OUT", "Outside");

    public CaseDetailModel SetCaseData(CaseDetailModel caseDetail, string groupCode, string detailCode, string value)
    {
        caseDetail = GetCaseDetail(caseDetail);
        var dataGroupDetails = caseDetail.DataGroupDetails.ToList();
        var dataGroupDetail = dataGroupDetails.FirstOrDefault(dgd => dgd.DataGroup.Code == groupCode);
        if (dataGroupDetail == null)
        {
            dataGroupDetail = new CaseDataGroupDetailModel
            (
                new CaseDataGroupModel
                (
                    ID: dataGroupID,
                    DataGroupDefinitionID: GetDataGroupDefinitionID(groupCode),
                    Code: groupCode,
                    Description: groupCode
                ),
                []
            );
            dataGroupID++;
        }
        dataGroupDetails.Remove(dataGroupDetail);
        var details = dataGroupDetail.Details.ToList();
        var detail = dataGroupDetail.Details.FirstOrDefault(d => d.Code == detailCode);
        if (detail == null)
        {
            detail = new CaseDataDetailModel
            (
                ID: dataGroupDetailID,
                DetailDefinitionID: GetDataGroupDetailDefinitionID(groupCode, detailCode),
                Code: detailCode,
                Description: detailCode,
                Value: ""
            );
            dataGroupDetailID++;
        }
        details.Remove(detail);
        var updatedDetail = detail with
        {
            Value = value
        };
        details.Add(updatedDetail);
        var updateDataGroupDetail = dataGroupDetail with
        {
            Details = details.ToArray()
        };
        dataGroupDetails.Add(updateDataGroupDetail);
        return UpdateCaseDetail
        (
            caseDetail,
            d => d with
            {
                DataGroupDetails = dataGroupDetails.ToArray()
            }
        );
    }

    public CaseDetailModel SetMapLayer(CaseDetailModel caseDetail, string layerName, string layerField, string value)
    {
        caseDetail = GetCaseDetail(caseDetail);
        var mapLayers = caseDetail.MapLayers.ToList();
        var mapLayer = mapLayers
            .FirstOrDefault(ml => ml.LayerName.Equals(layerName, StringComparison.OrdinalIgnoreCase) && ml.LayerField.Equals(layerField, StringComparison.OrdinalIgnoreCase));
        if (mapLayer == null)
        {
            mapLayer = new CwMapLayerModel(layerName, layerField, value);
            mapLayers.Add(mapLayer);
        }
        else
        {
            mapLayers.Remove(mapLayer);
            mapLayers.Add
            (
                mapLayer with { Value = value }
            );
        }
        return UpdateCaseDetail
        (
            caseDetail,
            d => d with
            {
                MapLayers = mapLayers.ToArray()
            }
        );
    }

    public CaseFeeModel AddCaseFee(CaseDetailModel caseDetail, string code, decimal amount)
    {
        caseDetail = GetCaseDetail(caseDetail);
        var feeDetails = caseDetail.FeeDetails.ToList();
        var feeDetail = new CaseFeeDetailModel
        (
            Fee: new
            (
                ID: caseFeeID,
                Code: code,
                Description: code,
                Amount: amount,
                PaymentAmount: 0
            ),
            AccountingCode: "",
            Payments: []
        );
        caseFeeID++;
        feeDetails.Add(feeDetail);
        UpdateCaseDetail
        (
            caseDetail,
            cd => caseDetail with
            {
                FeeDetails = feeDetails.ToArray()
            }
        );
        return feeDetail.Fee;
    }

    public Task<CasePaymentModel> AddCasePayment(AddCasePaymentRequest addRequest, CancellationToken ct)
    {
        var caseDetail = GetCaseDetail(addRequest.CaseID);
        var feeDetails = caseDetail.FeeDetails.ToList();
        var feeDetail = caseDetail.FeeDetails.First(f => f.Fee.ID == addRequest.CaseFeeID);
        var payments = feeDetail.Payments.ToList();
        var payment = new CasePaymentModel
        (
            ID: casePaymentID,
            PaymentDate: addRequest.TimePaid,
            PaymentAmount: addRequest.AmountPaid,
            TenderType: tenderTypes.FirstOrDefault(tt => tt.ID == addRequest.TenderTypeID)?.Description ?? "",
            ReferenceInfo: addRequest.ReferenceInfo,
            Comment: addRequest.CommentText
        );
        casePaymentID++;
        payments.Add(payment);
        feeDetails.Remove(feeDetail);
        feeDetail = feeDetail with
        {
            Fee = feeDetail.Fee with
            {
                PaymentAmount = payments.Sum(p => p.PaymentAmount)
            },
            Payments = payments.ToArray()
        };
        feeDetails.Add(feeDetail);
        UpdateCaseDetail
        (
            caseDetail,
            cd => caseDetail with
            {
                FeeDetails = feeDetails.ToArray()
            }
        );
        return Task.FromResult(payment);
    }

    public CaseFeeModel AddWaterCapacityFee(CaseDetailModel caseDetail) =>
        AddCaseFee(caseDetail, "WATER CAP", 800);

    public CaseFeeModel AddWaterTapFee(CaseDetailModel caseDetail) =>
        AddCaseFee(caseDetail, "WATER TAP", 1125);

    public CaseFeeModel AddIrrigationCapacityFee(CaseDetailModel caseDetail) =>
        AddCaseFee(caseDetail, "IRR CAP", 800);

    public CaseFeeModel AddIrrigationTapFee(CaseDetailModel caseDetail) =>
        AddCaseFee(caseDetail, "IRR TAP", 1125);

    public CaseFeeModel AddFireHydrantFee(CaseDetailModel caseDetail) =>
        AddCaseFee(caseDetail, "FIRE HYD", 3750);

    public CaseFeeModel AddElectricFee(CaseDetailModel caseDetail) =>
        AddCaseFee(caseDetail, "EL DEV", 400);

    public CaseFeeModel AddGasServiceFee(CaseDetailModel caseDetail) =>
        AddCaseFee(caseDetail, "GAS FEES", 255);

    public CaseFeeModel AddSewerCapacityFee(CaseDetailModel caseDetail) =>
        AddCaseFee(caseDetail, "SEWER CAP", 1200);

    public CaseFeeModel AddSewerTapFee(CaseDetailModel caseDetail) =>
        AddCaseFee(caseDetail, "SEWER TAP", 500);

    public CaseTaskModel AddWaterPaidTask(CaseDetailModel caseDetail) =>
        AddCaseTaskDetail(caseDetail, "WATER_PAID");

    public CaseTaskModel AddFireHydrantPaidTask(CaseDetailModel caseDetail) =>
        AddCaseTaskDetail(caseDetail, "HYD_PAID");

    public CaseTaskModel AddElectricPaidTask(CaseDetailModel caseDetail) =>
        AddCaseTaskDetail(caseDetail, "ELEC_PAID");

    public CaseTaskModel AddGasPaidTask(CaseDetailModel caseDetail) =>
        AddCaseTaskDetail(caseDetail, "GAS_PAID");

    public CaseTaskModel AddSewerPaidTask(CaseDetailModel caseDetail) =>
        AddCaseTaskDetail(caseDetail, "SEWER_PAID");

    public CaseTaskModel AddCaseTaskDetail(CaseDetailModel caseDetail, string taskCode)
    {
        caseDetail = GetCaseDetail(caseDetail);
        var taskDetails = caseDetail.TaskDetails.ToList();
        var taskDetail = new CaseTaskDetailModel
        (
            Task: new
            (
                ID: caseTaskID,
                LocationNotes: "",
                TargetStartDate: DateTimeOffset.MaxValue,
                TargetEndDate: DateTimeOffset.MaxValue,
                ActualStartDate: DateTimeOffset.MaxValue,
                ActualEndDate: DateTimeOffset.MaxValue,
                TimeCompleted: DateTimeOffset.MaxValue,
                ResultCode: "",
                StartPoint: 0,
                EndPoint: 0
            ),
            TaskTemplate: new
            (
                ID: GetTaskID(taskCode),
                Code: taskCode,
                Description: taskCode,
                ResultSetID: 0,
                TaskType: ""
            ),
            Results: [],
            Comments: []
        );
        taskDetails.Add(taskDetail);
        caseTaskID++;
        UpdateCaseDetail
        (
            caseDetail,
            d => d with
            {
                TaskDetails = taskDetails.ToArray()
            }
        );
        return taskDetail.Task;
    }

    private int GetTaskID(string taskCode) =>
        taskCode == "WT_PAYGO" ? 1 :
        taskCode == "SW_PAYGO" ? 2 :
        taskCode == "SW_PAYGO_2" ? 3 :
        taskCode == "HYD_PAYGO" ? 4 :
        taskCode == "ELEC_PAYG" ? 5 :
        taskCode == "GAS_PAYGO" ? 6 :
        0;

    private int GetDataGroupDefinitionID(string groupCode) =>
        groupCode == "CUSTOMER" ? 1 :
        groupCode == "INTERNAL" ? 2 :
        0;

    private int GetDataGroupDetailDefinitionID(string groupCode, string detailCode) =>
        groupCode == "CUSTOMER" && detailCode == "COMM_RES" ? 1 :
        groupCode == "CUSTOMER" && detailCode == "CUS DEV NO" ? 2 :
        groupCode == "CUSTOMER" && detailCode == "IN OR OUT" ? 3 :
        groupCode == "INTERNAL" && detailCode == "SVC LOCATE" ? 4 :
        0;

    public Task<CaseDetailModel> GetCaseDetail(long caseID, CancellationToken ct)
    {
        var caseDetail = GetCaseDetail(caseID);
        return Task.FromResult(caseDetail);
    }

    public Task<CaseModel> GetCase(long caseID, CancellationToken ct)
    {
        var caseDetail = GetCaseDetail(caseID);
        return Task.FromResult(caseDetail.Case);
    }

    public CaseDetailModel GetCaseDetail(CaseDetailModel caseDetail) =>
        GetCaseDetail(caseDetail.Case.ID);

    private CaseDetailModel GetCaseDetail(long caseID) => caseDetails.FirstOrDefault(c => c.Case.ID == caseID) ?? new();

    public Task UpdateCaseDataDetail(SaveCaseDataGroupDetailRequest updateRequest, CancellationToken ct)
    {
        var detail = caseDetails
            .SelectMany(c => c.DataGroupDetails)
            .SelectMany(d => d.Details)
            .FirstOrDefault(d => d.ID == updateRequest.DataGroupDetailID) ?? new();
        var caseDetail = caseDetails.FirstOrDefault(c => c.DataGroupDetails.SelectMany(d => d.Details).Contains(detail)) ?? new();
        var group = caseDetail.DataGroupDetails.FirstOrDefault(d => d.Details.Contains(detail)) ?? new();
        SetCaseData(caseDetail, group.DataGroup.Code, detail.Code, updateRequest.Value);
        return Task.CompletedTask;
    }

    public Task<CaseTaskModel> ResolveCaseTask(ResolveCaseTaskRequest resolveRequest, CancellationToken ct)
    {
        var taskDetail = caseDetails
            .SelectMany(t => t.TaskDetails)
            .FirstOrDefault(t => t.Task.ID == resolveRequest.ID) ??
            new();
        var caseDetail = caseDetails.FirstOrDefault(c => c.TaskDetails.Contains(taskDetail)) ?? new();
        var taskDetails = caseDetail.TaskDetails.ToList();
        taskDetails.Remove(taskDetail);
        taskDetail = taskDetail with
        {
            Task = taskDetail.Task with
            {
                ResultCode = resolveRequest.ResultCode
            }
        };
        taskDetails.Add(taskDetail);
        UpdateCaseDetail
        (
            caseDetail,
            d => d with
            {
                TaskDetails = taskDetails.ToArray()
            }
        );
        return Task.FromResult(taskDetail.Task);
    }

    public CaseDetailModel UpdateCaseDetail(CaseDetailModel caseDetail, Func<CaseDetailModel, CaseDetailModel> transform)
    {
        caseDetails.RemoveAll(c => c.Case.ID == caseDetail.Case.ID);
        var updatedCaseDetail = transform(caseDetail);
        caseDetails.Add(updatedCaseDetail);
        return updatedCaseDetail;
    }

    public Task<CustomFieldCategoryModel[]> GetCustomFieldCategories(CancellationToken ct) =>
        Task.FromResult(customFieldCategories.ToArray());

    private readonly Dictionary<long, CaseReceiptDetailModel> caseReceipts = new();

    public Task<CaseReceiptDetailModel> AddCaseReceipt(AddCaseReceiptRequest addRequest, CancellationToken ct)
    {
        if (caseReceipts.ContainsKey(addRequest.CaseID))
        {
            caseReceipts.Remove(addRequest.CaseID);
        }
        var caseDetail = GetCaseDetail(addRequest.CaseID);
        var payments = addRequest.CasePaymentIDs.Select(pid => caseDetail.GetPaymentOrDefault(pid)).ToArray();
        var receiptDetail = new CaseReceiptDetailModel
        (
            Receipt: new
            (
                ID: receiptID,
                ReceiptDate: DateTime.Now,
                TotalAmountDue: 0,
                TotalAmountTendered: payments.Any() ? payments.Sum(p => p.PaymentAmount) : 0,
                FileName: $"PYMT_{addRequest.PaymentTransactionID}"
            ),
            TenderTypes: [],
            Fees: []
        );
        receiptDetails.Add(receiptDetail);
        caseReceipts.Add(addRequest.CaseID, receiptDetail);
        receiptID++;
        return Task.FromResult(receiptDetail);
    }

    public CaseReceiptDetailModel GetReceiptDetail(CaseDetailModel caseDetail)
    {
        if(!caseReceipts.TryGetValue(caseDetail.Case.ID, out var receiptDetail))
        {
            receiptDetail = new CaseReceiptDetailModel();
        }
        return receiptDetail;
    }

    private Dictionary<long, byte[]> receiptFiles = new();

    public Task UploadCaseReceiptFile(UploadCaseReceiptRequest uploadRequest, CancellationToken ct)
    {
        receiptFiles.Add(uploadRequest.ReceiptID, uploadRequest.ReceiptFile.Stream.GetBytes());
        return Task.CompletedTask;
    }

    public byte[] GetReceiptFile(CaseReceiptDetailModel receiptDetail)
    {
        if(!receiptFiles.TryGetValue(receiptDetail.Receipt.ID, out var bytes))
        {
            bytes = [];
        }
        return bytes;
    }
}
