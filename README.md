# SEPA-Batch-Generator
SEPA XML Payment Batch Generator

This SEPA XML generator can be used to generate payment XML files for European banks.

### How To Use
- Copy the class class_sepa_generator.vb into your App_Code folder in your project
- Init the class and execute the function oSEPA.GenerateSepaXML() to generate your SEPA XML file

### Usage Example
```
Dim oSEPA As New cSepaXML
oSEPA.customer_data = oDataTable   <-- add your data object in here
Dim sXml As String = oSEPA.GenerateSepaXML()
```

Check a example here: [https://github.com/jovana/SEPA-Batch-Generator/blob/master/sepa_test.aspx](sepa_test.aspx)


### Output Example
```
<?xml version="1.0" encoding="utf-8"?>
<Document xmlns="urn:iso:std:iso:20022:tech:xsd:pain.008.001.02" xmlns:xsi="www.w3.org/2001/XMLSchema-instance">
	<CstmrDrctDbtInitn>
		<GrpHdr>
			<MsgId>
				1c05f16c26bd0aa1295d7f95e07e8bf49552f443
			</MsgId>
			<CreDtTm>
				2018-10-18T13:10:48
			</CreDtTm>
			<NbOfTxs>
				2
			</NbOfTxs>
			<CtrlSum>
				33.24
			</CtrlSum>
			<InitgPty>
				<Nm>
				</Nm>
			</InitgPty>
		</GrpHdr>
		<PmtInf>
			<PmtInfId>
				1c05f16c26bd0aa1295d7f95e07e8bf49552f443FRST
			</PmtInfId>
			<PmtMtd>
				DD
			</PmtMtd>
			<NbOfTxs>
				2
			</NbOfTxs>
			<CtrlSum>
				33.24
			</CtrlSum>
			<PmtTpInf>
				<SvcLvl>
					<Cd>
						SEPA
					</Cd>
				</SvcLvl>
				<LclInstrm>
					<Cd>
						CORE
					</Cd>
				</LclInstrm>
				<SeqTp>
					FRST
				</SeqTp>
			</PmtTpInf>
			<ReqdColltnDt>
				2018-10-18
			</ReqdColltnDt>
			<Cdtr>
				<Nm>
				</Nm>
			</Cdtr>
			<CdtrAcct>
				<Id>
					<IBAN>
						YOUR_IBAN
					</IBAN>
				</Id>
			</CdtrAcct>
			<CdtrAgt>
				<FinInstnId>
					<BIC>
						YOUR_BIC_CODE
					</BIC>
				</FinInstnId>
			</CdtrAgt>
			<ChrgBr>
				SLEV
			</ChrgBr>
			<DrctDbtTxInf>
				<PmtId>
					<EndToEndId>
						cust.123.456
					</EndToEndId>
				</PmtId>
				<InstdAmt Ccy="EUR">
					10.12
				</InstdAmt>
				<DrctDbtTx>
					<MndtRltdInf>
						<MndtId>
							4001
						</MndtId>
						<DtOfSgntr>
							2018-01-01
						</DtOfSgntr>
					</MndtRltdInf>
					<CdtrSchmeId>
						<Id>
							<PrvtId>
								<Othr>
									<Id>
										YOURID_123345456
									</Id>
									<SchmeNm>
										<Prtry>
											SEPA
										</Prtry>
									</SchmeNm>
								</Othr>
							</PrvtId>
						</Id>
					</CdtrSchmeId>
				</DrctDbtTx>
				<DbtrAgt>
					<FinInstnId>
						<BIC>
							BIC-CODE
						</BIC>
					</FinInstnId>
				</DbtrAgt>
				<Dbtr>
					<Nm>
						Customer Name A
					</Nm>
					<PstlAdr>
						<Ctry>
							NL
						</Ctry>
						<AdrLine>
							Street 1
						</AdrLine>
						<AdrLine>
							1234AB Amsterdam
						</AdrLine>
					</PstlAdr>
				</Dbtr>
				<DbtrAcct>
					<Id>
						<IBAN>
							IBAN-NUMBER
						</IBAN>
					</Id>
				</DbtrAcct>
				<RmtInf>
					<Ustrd>
						Invoice.1234 KL-4001 Your fixed description
					</Ustrd>
				</RmtInf>
			</DrctDbtTxInf>
			<DrctDbtTxInf>
				<PmtId>
					<EndToEndId>
						cust.456.789
					</EndToEndId>
				</PmtId>
				<InstdAmt Ccy="EUR">
					23.12
				</InstdAmt>
				<DrctDbtTx>
					<MndtRltdInf>
						<MndtId>
							4002
						</MndtId>
						<DtOfSgntr>
							2018-01-01
						</DtOfSgntr>
					</MndtRltdInf>
					<CdtrSchmeId>
						<Id>
							<PrvtId>
								<Othr>
									<Id>
										YOURID_123345456
									</Id>
									<SchmeNm>
										<Prtry>
											SEPA
										</Prtry>
									</SchmeNm>
								</Othr>
							</PrvtId>
						</Id>
					</CdtrSchmeId>
				</DrctDbtTx>
				<DbtrAgt>
					<FinInstnId>
						<BIC>
							BIC-CODE
						</BIC>
					</FinInstnId>
				</DbtrAgt>
				<Dbtr>
					<Nm>
						Customer Name B
					</Nm>
					<PstlAdr>
						<Ctry>
							NL
						</Ctry>
						<AdrLine>
							Street 3
						</AdrLine>
						<AdrLine>
							1234AB Amsterdam
						</AdrLine>
					</PstlAdr>
				</Dbtr>
				<DbtrAcct>
					<Id>
						<IBAN>
							IBAN-NUMBER
						</IBAN>
					</Id>
				</DbtrAcct>
				<RmtInf>
					<Ustrd>
						Invoice.2345 KL-4002 Your fixed description
					</Ustrd>
				</RmtInf>
			</DrctDbtTxInf>
		</PmtInf>
		<PmtInf>
			<PmtInfId>
				1c05f16c26bd0aa1295d7f95e07e8bf49552f443RCUR
			</PmtInfId>
			<PmtMtd>
				DD
			</PmtMtd>
			<NbOfTxs>
				0
			</NbOfTxs>
			<CtrlSum>
				0
			</CtrlSum>
			<PmtTpInf>
				<SvcLvl>
					<Cd>
						SEPA
					</Cd>
				</SvcLvl>
				<LclInstrm>
					<Cd>
						CORE
					</Cd>
				</LclInstrm>
				<SeqTp>
					RCUR
				</SeqTp>
			</PmtTpInf>
			<ReqdColltnDt>
				2018-10-18
			</ReqdColltnDt>
			<Cdtr>
				<Nm>
				</Nm>
			</Cdtr>
			<CdtrAcct>
				<Id>
					<IBAN>
						YOUR_IBAN
					</IBAN>
				</Id>
			</CdtrAcct>
			<CdtrAgt>
				<FinInstnId>
					<BIC>
						YOUR_BIC_CODE
					</BIC>
				</FinInstnId>
			</CdtrAgt>
			<ChrgBr>
				SLEV
			</ChrgBr>
		</PmtInf>
	</CstmrDrctDbtInitn>
</Document>

```

