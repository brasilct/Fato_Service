USE BookingDB_ERP_BC
GO

SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
GO

CREATE PROC [dbo].[SP_FATO_Insert_update]
    @FromDate DATETIME = NULL ,
    @ToDate DATETIME = NULL ,
    @Booking_ref VARCHAR(10) = NULL ,
    @CMPNY_ID VARCHAR(10) ,
    @QUERY TINYINT ,
    @branch_Id AS VARCHAR(8000) = NULL
AS
    BEGIN
        IF @QUERY = 1 -- query to get Fato Insert Date  for company
            BEGIN
                SELECT TOP 1
                        Fato_Insert_Date
                FROM    FATO_Service_Setting
                WHERE   Company_ID = @CMPNY_ID
            END
        IF @QUERY = 2 -- query to INSERT Data in FATO RECORDS  
            BEGIN
                DECLARE @Record_Count_before INT
					,	@Record_Count_After INT

                SELECT  @Record_Count_before = COUNT(*)
                FROM    FATO_Records (NOLOCK)

                EXEC FSP_UpdIns_FATO_Records @FromDate, @ToDate, @CMPNY_ID, 'ALL', 1, @Booking_ref

                SELECT  @Record_Count_After = COUNT(*)
                FROM    FATO_Records (NOLOCK)

                IF @Record_Count_After > @Record_Count_before
					BEGIN
                        UPDATE  FATO_Service_Setting
                        SET     Fato_Insert_Date = @ToDate
                        WHERE   Company_ID = @CMPNY_ID
                    END
            END
        IF @QUERY = 3 -- query to Update Data in FATO_Records / Also checks for refs that were not originally added to FATO_Records table
            BEGIN
                CREATE TABLE #FATO_Records_update
                    (
                      RecordId INT IDENTITY(1, 1) ,
                      Booking_Ref VARCHAR(20)
                    )

                INSERT  INTO #FATO_Records_update
                        ( Booking_Ref )
                SELECT  Booking_Ref
                FROM    FATO_Records (NOLOCK)
                WHERE   Change_Status = 1
				UNION -- missing refs
                SELECT  booking_ref
				FROM    dbo.booking_master (NOLOCK)
				WHERE   date_of_booking > '20180101'
						AND booking_ref NOT IN ( SELECT Booking_Ref
												 FROM   dbo.FATO_Records (NOLOCK))

                DECLARE @Record_Count INT
					,	@New_Record_Count INT
					,	@Record_No INT

                SET @Record_No = 1

                SELECT  @Record_Count = COUNT(*)
                FROM    #FATO_Records_update
                
				WHILE @Record_No <= @Record_Count
                    BEGIN
                        SELECT  @Booking_ref = ( SELECT Booking_Ref
                                                 FROM   #FATO_Records_update
                                                 WHERE  RecordId = @Record_No
                                               )

                        EXEC FSP_UpdIns_FATO_Records @FromDate, @ToDate, @CMPNY_ID, 'ALL', 1, @Booking_ref

                        SET @Record_No = @Record_No + 1
                    END
            END
        IF @QUERY = 4 -- query to INSERT Data in FATO RECORDS
            BEGIN
                UPDATE  FATO_Records
                SET     Change_Status = 1
                WHERE   Booking_Ref = @Booking_ref
            END
        IF @QUERY = 5 -- query to Get Data from FATO RECORDS Table
            BEGIN
                IF ISNULL(@branch_Id, '') = ''
                    BEGIN
                        SET @branch_Id = ( SELECT	STUFF((SELECT ', ' + CAST(branch_id AS VARCHAR(10)) [text()]
                                                           FROM	company_branch (NOLOCK)
                                                           WHERE company_id = @CMPNY_ID
														   FOR   XML PATH('') , TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ' ')
                                         )
                    END

				--Erase Alias, If exists in DB
				IF Object_ID('tempdb..#tmp_tab_fato_query_5') IS NOT NULL
				DROP TABLE #tmp_tab_fato_query_5

                SELECT  
						DISTINCT
                        Booking_Ref ,
                        Company_id ,
                        ISNULL(Affiliate_Id, '') Affiliate_Id ,
                        ISNULL(Product, '') Product ,
                        ISNULL(CPF_No, '') CPF_No ,
                        ISNULL(Booking_Date, '') Booking_Date ,
                        ISNULL(Allocation_Date, '') Allocation_Date ,
                        ISNULL(Trip_Start_Date, '') Trip_Start_Date ,
                        ISNULL(MarketType, '') MarketType ,
                        ISNULL(Trip_End_Date, '') Trip_End_Date ,
                        ISNULL(City, '') City ,
                        ISNULL(No_of_Passenger, 0) No_of_Passenger ,
                        ISNULL(Sales_Channel, '') Sales_Channel ,
                        ISNULL(CPP, 0) CPP ,
                        ISNULL(payment_type, '') payment_type ,
                        ISNULL(Lead_Pax, '') Lead_Pax ,
                        ISNULL(Status, '') Status ,
                        ISNULL(Supplier_Code1, '') Supplier_Code1 ,
                        ISNULL(Supplier_Code2, '') Supplier_Code2 ,
                        ISNULL(Supplier_Code3, '') Supplier_Code3 ,
                        ISNULL(Supplier_Code4, '') Supplier_Code4 ,
                        ISNULL(PNR_No1, '') PNR_No1 ,
                        ISNULL(PNR_No2, '') PNR_No2 ,
                        ISNULL(PNR_No3, '') PNR_No3 ,
                        ISNULL(PNR_No4, '') PNR_No4 ,
                        ISNULL(Itinerary_Detail, '') Itinerary_Detail ,
                        ISNULL(Hotel_Exchange_Rate, 0) Hotel_Exchange_Rate ,
                        ISNULL(NetRate1, 0) NetRate1 ,
                        ISNULL(NetRate2, 0) NetRate2 ,
                        ISNULL(NetRate3, 0) NetRate3 ,
                        ISNULL(NetRate4, 0) NetRate4 ,
                        ISNULL(Tax, 0) Tax ,
                        ISNULL(RAV, 0) RAV ,
                        ISNULL(Supplier_Fee1, 0) Supplier_Fee1 ,
                        ISNULL(Supplier_Fee2, 0) Supplier_Fee2 ,
                        ISNULL(Supplier_Fee3, 0) Supplier_Fee3 ,
                        ISNULL(Supplier_Fee4, 0) Supplier_Fee4 ,
                        ISNULL(Trans_Fee1, 0) Trans_Fee1 ,
                        ISNULL(Trans_Fee2, 0) Trans_Fee2 ,
                        ISNULL(Trans_Fee3, 0) Trans_Fee3 ,
                        ISNULL(Trans_Fee4, 0) Trans_Fee4 ,
                        ISNULL(Total_Value_Supplier, 0) Total_Value_Supplier ,
                        ISNULL(Total_Value_Charged, 0) Total_Value_Charged ,
                        ISNULL(Bonus_Points, 0) Bonus_Points ,
                        ISNULL(Cash, 0) Cash ,
                        ISNULL(CC_Amount1, 0) CC_Amount1 ,
                        ISNULL(Installment_Amount1, 0) Installment_Amount1 ,
                        ISNULL(Trans_ID1, '') Trans_ID1 ,
                        ISNULL(CC_Amount2, 0) CC_Amount2 ,
                        ISNULL(Installment_Amount2, 0) Installment_Amount2 ,
                        ISNULL(Trans_ID2, '') Trans_ID2 ,
                        ISNULL(CC_Amount3, 0) CC_Amount3 ,
                        ISNULL(Installment_Amount3, 0) Installment_Amount3 ,
                        ISNULL(Trans_ID3, '') Trans_ID3 ,
                        ISNULL(CC_Amount4, 0) CC_Amount4 ,
                        ISNULL(Installment_Amount4, 0) Installment_Amount4 ,
                        ISNULL(Trans_ID4, '') Trans_ID4 ,
                        ISNULL(Itn_Status1, '') Itn_Status1 ,
                        ISNULL(Itn_Status2, '') Itn_Status2 ,
                        ISNULL(Itn_Status3, '') Itn_Status3 ,
                        ISNULL(Itn_Status4, '') Itn_Status4 ,
                        ISNULL(CC_Company1, '') CC_Company1 ,
                        ISNULL(CC_Company2, '') CC_Company2 ,
                        ISNULL(CC_Company3, '') CC_Company3 ,
                        ISNULL(CC_Company4, '') CC_Company4 ,
                        ISNULL(CC_AmountRecurrence1, 0) CC_AmountRecurrence1 ,
                        ISNULL(Installment_AmountRecurrence1, 0) Installment_AmountRecurrence1 ,
                        ISNULL(Trans_IDRecurrence1, '') Trans_IDRecurrence1 ,
                        ISNULL(CC_AmountRecurrence2, 0) CC_AmountRecurrence2 ,
                        ISNULL(Installment_AmountRecurrence2, 0) Installment_AmountRecurrence2 ,
                        ISNULL(Trans_IDRecurrence2, '') Trans_IDRecurrence2 ,
                        ISNULL(CC_AmountRecurrence3, 0) CC_AmountRecurrence3 ,
                        ISNULL(Installment_AmountRecurrence3, 0) Installment_AmountRecurrence3 ,
                        ISNULL(Trans_IDRecurrence3, '') Trans_IDRecurrence3 ,
                        ISNULL(CC_AmountRecurrence4, 0) CC_AmountRecurrence4 ,
                        ISNULL(Installment_AmountRecurrence4, 0) Installment_AmountRecurrence4 ,
                        ISNULL(Trans_IDRecurrence4, '') Trans_IDRecurrence4 ,
                        ISNULL(Discount_Amount1, 0) Discount_Amount1 ,
                        ISNULL(Discount_Amount2, 0) Discount_Amount2 ,
                        ISNULL(Discount_Amount3, 0) Discount_Amount3 ,
                        ISNULL(Discount_Type1, '') Discount_Type1 ,
                        ISNULL(Discount_Type2, '') Discount_Type2 ,
                        ISNULL(Discount_Type3, '') Discount_Type3 ,
                        ISNULL(branch_id, '') branch_id
				INTO #tmp_tab_fato_query_5 
                FROM    FATO_Records (NOLOCK)
                WHERE   Booking_Date >= @FromDate
                        AND Booking_Date < DATEADD(DAY, 1, @ToDate)
                        AND branch_id IN (
                        SELECT  value
                        FROM    dbo.fnSeprator(@branch_Id, ',') )

				SELECT
					ROW_NUMBER() OVER (ORDER BY Booking_Ref ASC ) AS Row# , *
					FROM #tmp_tab_fato_query_5
            END
    END
GO
