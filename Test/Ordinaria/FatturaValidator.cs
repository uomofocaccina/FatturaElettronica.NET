﻿using System.Linq;
using FatturaElettronica;
using FatturaElettronica.Common;
using FatturaElettronica.Ordinaria;
using FatturaElettronica.Ordinaria.FatturaElettronicaBody;
using FatturaElettronica.Ordinaria.FatturaElettronicaBody.DatiBeniServizi;
using FatturaElettronica.Ordinaria.FatturaElettronicaBody.DatiGenerali;
using FluentValidation.TestHelper;
using Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ordinaria.Tests
{
    [TestClass]
    public class FatturaValidator : BaseClass<FatturaOrdinaria, FatturaElettronica.Validators.FatturaOrdinariaValidator>
    {
        [TestInitialize]
        public new void Init()
        {
            validator = new FatturaElettronica.Validators.FatturaOrdinariaValidator();
            challenge = new FatturaOrdinaria();
        }

        [TestMethod]
        public void FatturaElettronicaHeaderHasChildValidator()
        {
            validator.ShouldHaveChildValidator(
                x => x.FatturaElettronicaHeader,
                typeof(FatturaElettronica.Validators.FatturaElettronicaHeaderValidator));
        }

        [TestMethod]
        public void FatturaElettronicaBodyHasChildValidator()
        {
            validator.ShouldHaveChildValidator(
                x => x.FatturaElettronicaBody, typeof(FatturaElettronica.Validators.FatturaElettronicaBodyValidator));
        }

        [TestMethod]
        public void FatturaValidateAgainstError00471()
        {
            var tipiDocumento = new string[] {"TD16", "TD17", "TD18", "TD19", "TD20"};
            var cedente = challenge.FatturaElettronicaHeader.CedentePrestatore.DatiAnagrafici;
            var cessionario = challenge.FatturaElettronicaHeader.CessionarioCommittente.DatiAnagrafici;
            var id123 = new IdFiscaleIVA {IdPaese = "IT", IdCodice = "123"};
            var id456 = new IdFiscaleIVA {IdPaese = "IT", IdCodice = "456"};

            foreach (var tipoDocumento in tipiDocumento)
            {
                var body = new FatturaElettronicaBody();
                body.DatiGenerali.DatiGeneraliDocumento.TipoDocumento = tipoDocumento;
                challenge.FatturaElettronicaBody.Add(body);

                cedente.CodiceFiscale = null;
                cessionario.CodiceFiscale = null;

                cedente.IdFiscaleIVA = id123;
                cessionario.IdFiscaleIVA = id123;
                var result = challenge.Validate();
                Assert.IsNotNull(result.Errors.FirstOrDefault(x => x.ErrorCode == "00471"));

                cedente.IdFiscaleIVA = id456;
                result = challenge.Validate();
                Assert.IsNull(result.Errors.FirstOrDefault(x => x.ErrorCode == "00471"));

                cedente.IdFiscaleIVA = new IdFiscaleIVA();
                cessionario.IdFiscaleIVA = new IdFiscaleIVA();
                cedente.CodiceFiscale = "123";
                cessionario.CodiceFiscale = "123";
                result = challenge.Validate();
                Assert.IsNotNull(result.Errors.FirstOrDefault(x => x.ErrorCode == "00471"));

                cessionario.CodiceFiscale = "456";
                result = challenge.Validate();
                Assert.IsNull(result.Errors.FirstOrDefault(x => x.ErrorCode == "00471"));
            }
        }

        [TestMethod]
        public void BodyValidateAgainstError00444()
        {
            var body = new FatturaElettronicaBody();
            body.DatiBeniServizi.DettaglioLinee.Add(new DettaglioLinee {Natura = "N1"});
            body.DatiBeniServizi.DettaglioLinee.Add(new DettaglioLinee {Natura = "N2"});
            body.DatiGenerali.DatiGeneraliDocumento.DatiCassaPrevidenziale.Add(
                new DatiCassaPrevidenziale {Natura = "N3"});
            body.DatiBeniServizi.DatiRiepilogo.Add(new DatiRiepilogo {Natura = "N1"});
            challenge.FatturaElettronicaBody.Add(body);

            var result = validator.Validate(challenge);
            Assert.IsNotNull(result.Errors.FirstOrDefault(x => x.ErrorCode == "00444"));

            body.DatiBeniServizi.DatiRiepilogo.Add(new DatiRiepilogo {Natura = "N2"});
            Assert.IsNotNull(result.Errors.FirstOrDefault(x => x.ErrorCode == "00444"));

            body.DatiBeniServizi.DatiRiepilogo.Add(new DatiRiepilogo {Natura = "N3"});
            result = validator.Validate(challenge);
            Assert.IsNull(result.Errors.FirstOrDefault(x => x.ErrorCode == "00444"));
        }

        [TestMethod]
        public void BodyValidateAgainstError00443()
        {
            var body = new FatturaElettronicaBody();
            body.DatiBeniServizi.DettaglioLinee.Add(new DettaglioLinee {AliquotaIVA = 1m});
            body.DatiBeniServizi.DettaglioLinee.Add(new DettaglioLinee {AliquotaIVA = 2m});
            body.DatiGenerali.DatiGeneraliDocumento.DatiCassaPrevidenziale.Add(
                new DatiCassaPrevidenziale {AliquotaIVA = 3m});
            body.DatiBeniServizi.DatiRiepilogo.Add(new DatiRiepilogo {AliquotaIVA = 1m});
            challenge.FatturaElettronicaBody.Add(body);

            var result = validator.Validate(challenge);
            Assert.IsNotNull(result.Errors.FirstOrDefault(x => x.ErrorCode == "00443"));

            body.DatiBeniServizi.DatiRiepilogo.Add(new DatiRiepilogo {AliquotaIVA = 2m});
            Assert.IsNotNull(result.Errors.FirstOrDefault(x => x.ErrorCode == "00443"));

            body.DatiBeniServizi.DatiRiepilogo.Add(new DatiRiepilogo {AliquotaIVA = 3m});
            result = validator.Validate(challenge);
            Assert.IsNull(result.Errors.FirstOrDefault(x => x.ErrorCode == "00443"));
        }
    }
}