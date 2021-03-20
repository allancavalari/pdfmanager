using GhostscriptSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PDFman
{
    public partial class PDFManager : Form
    {
        //Criar coleção de PDF (datasource)
        List<Arquivo> pdfLista = new List<Arquivo>();

        //Thumbs
        string thumbDir = AppDomain.CurrentDomain.BaseDirectory + @"thumb\";
        string pathFile = AppDomain.CurrentDomain.BaseDirectory + @"libPath";
        string keepCache = AppDomain.CurrentDomain.BaseDirectory + @"keepCache";

        Image Miniatura;

        public void carregar(string pasta)
        {
            progressBar1.Visible = true;

            resetProp();
            setProp(false);

            dataGridView1.DataSource = null;
            pdfLista.Clear();

            //Adicionar PDFs
            string[] aquivosPDFs = Directory.GetFiles(pasta, "*.pdf", SearchOption.AllDirectories);

            //Adicionar categorias (subdiretórios)
            carregarCategorias();
            if (pasta == textboxDir.Text) { dropdownCategorias.Text = @"\"; }

            //Configura progressbar
            progressBar1.Maximum = aquivosPDFs.Length;
            progressBar1.Value = progressBar1.Minimum;
            
            userInfoMsg("Analyzing " + aquivosPDFs.Length.ToString() + " files");
            Image thumbImg = null;

            int contPdf = 0;
            foreach (string fileName in aquivosPDFs)
            {
                bool pwdPDF = false;
                try
                {
                    //Verificar se o PDF está protegido
                    pwdPDF = IsPasswordProtected(fileName);

                    //Abre PDF para leitura
                    PdfDocument document = PdfReader.Open(fileName, PdfDocumentOpenMode.ReadOnly);
                
                    //Adiciona novo arquivo na lista
                    thumbImg = Image.FromFile(gerarThumb(fileName));

                    //Adiciona PDF à coleção
                    pdfLista.Add(new Arquivo(thumbImg,
                                                 Path.GetFileName(document.FullPath),
                                                 String.Empty,
                                                 document.Info.Title,
                                                 document.Info.Author,
                                                 document.FullPath,
                                                 pwdPDF,
                                                 false,
                                                 false));
                    //Fechar PDF
                    document.Close();
                    document.Dispose();

                    //Incrementa contador
                    contPdf++;
                }
                catch
                {
                    userInfoMsg("!Error opening file: " + Path.GetFileName(fileName));
                }

                //Indica o progresso
                progressBar1.PerformStep();
                Application.DoEvents();
            }

            thumbImg = null;

            //Popula e ajusta datagrid
            dataGridView1.DataSource = pdfLista;
            dataGridView1.Columns["read_only"].Visible = false;
            dataGridView1.Columns["deletar"].Visible = false;
            dataGridView1.Columns["modificar"].Visible = false;
            dataGridView1.Columns["Arquivo_FullPath"].Visible = false;

            reajustarGrid();

            //Oculta a progressbar
            progressBar1.Visible = false;

            //Ativa/Desativa botões e limpa campos
            resetProp();
            setProp(false);
            if (dataGridView1.Rows.Count > 0)
            {
                btGenFNames.Enabled = true;
                selecionarPDF();
            }
            else
            {
                setProp(false);
                btGenFNames.Enabled = false;
            }

            btSave.Enabled = false;

            //Salvar path da Lib
            System.IO.File.WriteAllText(pathFile, textboxDir.Text);

            //Fim
            userInfoMsg("Done: " + contPdf.ToString() + " files loaded");
        }

        // Rotina para salvar modificações
        public void salvar()
        {
            userInfoMsg("Saving...");

            //Configura progressbar
            progressBar1.Maximum = pdfLista.Count;
            progressBar1.Value = progressBar1.Minimum;
            progressBar1.Visible = true;

            foreach (Arquivo x in pdfLista)
            {
                string filePath = x.Arquivo_FullPath;
                string fileNewPath = x.Arquivo_FullPath;

                // Se o arquivo não está marcado para exclusão
                if (x.deletar == false)
                {
                    // Se há novo nome para o arquivo, renomeia.
                    if (!String.IsNullOrEmpty(x.Arquivo_Novo_Nome))
                    {
                        fileNewPath = Path.GetDirectoryName(x.Arquivo_FullPath) + @"\" + x.Arquivo_Novo_Nome; // Novo filepath

                        userInfoMsg("Renaming the file: <" + Path.GetFileName(filePath) + "> to: <" + Path.GetFileName(fileNewPath) + ">");

                        try
                        {
                            System.IO.File.Move(filePath, fileNewPath);
                            filePath = fileNewPath;
                        }
                        catch
                        {
                            userInfoMsg("!Error: Failed to rename the file <" + Path.GetFileName(filePath) + ">");
                        }

                    }

                    // Salvar propriedades
                    if ((x.modificar == true) && (x.read_only == false))
                    { 
                        try
                        {
                            userInfoMsg("Saving tags to the file <" + Path.GetFileName(filePath) + ">");
                            PdfDocument document = PdfReader.Open(filePath);
                            document.Info.Title = x.Titulo;
                            document.Info.Author = x.Autor;
                            document.Save(filePath);
                            document.Close();
                            document.Dispose();
                        }
                        catch
                        {
                            userInfoMsg("!Error: Failed to save changes to the file <" + Path.GetFileName(filePath) + ">");
                        }
                    }
                } // fim da rotina de alteração de filename e salvar alterações
                else
                {
                    try
                    {
                        userInfoMsg("Deleting the file <" + Path.GetFileName(x.Arquivo_FullPath) + ">");
                        File.Delete(x.Arquivo_FullPath);
                    }
                    catch
                    {
                        if (File.Exists(x.Arquivo_FullPath)) { userInfoMsg("!Error deleting selected PDF"); }
                    }
                }

                progressBar1.PerformStep();
                Application.DoEvents();
            }
            carregar(pastaAtual());
            userInfoMsg("Saving complete");
            btSave.Enabled = false;
        }

        // Rotina para gerar novos nomes para os arquivos
        private void gerNomes(int formato)
        {
            userInfoMsg("Generating new names...");
            progressBar1.Maximum = pdfLista.Count;
            progressBar1.Value = progressBar1.Minimum;
            progressBar1.Visible = true;

            string separador = " - ";

            foreach (Arquivo x in pdfLista)
            {
                if (!string.IsNullOrEmpty(x.Titulo))
                {
                    if (string.IsNullOrEmpty(x.Autor)) { separador = ""; }
                    else { separador = " - "; }

                    switch (formato)
                    {
                        case 1:
                            x.Arquivo_Novo_Nome = retirarInvChar(x.Autor + separador + x.Titulo + ".pdf");
                            break;

                        case 2:
                            x.Arquivo_Novo_Nome = retirarInvChar(x.Titulo + separador + x.Autor + ".pdf");
                            break;

                        case 3:
                            x.Arquivo_Novo_Nome = retirarInvChar(x.Titulo + " (" + x.Autor + ").pdf");
                            break;

                        case 4:
                            x.Arquivo_Novo_Nome = retirarInvChar(x.Titulo + ".pdf");
                            break;

                        default:
                            x.Arquivo_Novo_Nome = retirarInvChar(x.Autor + separador + x.Titulo + ".pdf");
                            break;
                    }
                    
                }
                progressBar1.PerformStep();
                Application.DoEvents();
            }

            dataGridView1.Refresh();
            btSave.Enabled = true;
            userInfoMsg("Process completed successfully");
            progressBar1.Value = progressBar1.Minimum;
            progressBar1.Visible = false;
        }

        // Rotina para retirar caracteres inválidos
        string retirarInvChar(string nomeDoArquivo)
        {
            string[] invalidChars = { @"\", @"/", @"|", @"<", @">", @"*", @":", @"“", @"?" };
            string nomeDoArquivoFinal = string.Empty;
            nomeDoArquivoFinal = nomeDoArquivo;
            foreach (string invChar in invalidChars)
            {
                nomeDoArquivoFinal = nomeDoArquivoFinal.Replace(invChar, " ");
            }
            nomeDoArquivoFinal = nomeDoArquivoFinal.Replace("  ", " ");
            nomeDoArquivoFinal = nomeDoArquivoFinal.Replace("   ", " ");
            nomeDoArquivoFinal = nomeDoArquivoFinal.Replace(" .pdf", ".pdf");
            return nomeDoArquivoFinal;
        }

        // Rotina para gerar novo nome para um arquivo
        private void gerNomeIndividual(int formato)
        {
            Arquivo x = PdfSelecionado();

            if (!string.IsNullOrEmpty(x.Autor) && !string.IsNullOrEmpty(x.Titulo))
            {
                switch (formato)
                {
                    case 1:
                        PdfSelecionado().Arquivo_Novo_Nome = retirarInvChar(x.Autor + " - " + x.Titulo + ".pdf");
                        break;

                    case 2:
                        PdfSelecionado().Arquivo_Novo_Nome = retirarInvChar(x.Titulo + " - " + x.Autor + ".pdf");
                        break;

                    case 3:
                        PdfSelecionado().Arquivo_Novo_Nome = retirarInvChar(x.Titulo + " (" + x.Autor + ").pdf");
                        break;

                    case 4:
                        PdfSelecionado().Arquivo_Novo_Nome = retirarInvChar(x.Titulo + ".pdf");
                        break;

                    default:
                        PdfSelecionado().Arquivo_Novo_Nome = retirarInvChar(x.Autor + " - " + x.Titulo + ".pdf");
                        break;
                } 
            }

            dataGridView1.Refresh();
            btSave.Enabled = true;
        }

        // Rotina para gerar Thumbnail
        string gerarThumb(string filename)
        {
            string inputPdf = filename;
            string outputPng = thumbDir + Path.GetFileNameWithoutExtension(filename) + ".png";
            string outputPngBig = thumbDir + Path.GetFileNameWithoutExtension(filename) + "_big.png";

            //Criar diretório de thumbnails, caso não exista
            if (!Directory.Exists(thumbDir)) { Directory.CreateDirectory(thumbDir);  }

            if (File.Exists(outputPng) && File.Exists(outputPngBig))
            {
                return outputPng;
            }
            else
            {
                if (!File.Exists(outputPng))
                {
                    try { GhostscriptWrapper.GeneratePageThumbs(inputPdf, outputPng, 1, 1, 10, 10); }
                    catch { userInfoMsg("!Error: Could not generate thumbnail for the file: " + Path.GetFileName(filename)); }
                }
                if (!File.Exists(outputPngBig))
                {
                    try { GhostscriptWrapper.GeneratePageThumbs(inputPdf, outputPngBig, 1, 1, 100, 100); }
                    catch { userInfoMsg("!Error: Could not generate preview of the file: " + Path.GetFileName(filename)); }
                }

                if (File.Exists(outputPng)) { return outputPng; }
                else { return string.Empty; }
            }
        }

        // Mensagem de status
        private void userInfoMsg(string msg)
        {
            textboxStatusBar.AppendText(msg);
            textboxStatusBar.AppendText(Environment.NewLine);
        }
        private void reajustarGrid()
        {
            if ((dataGridView1.RowCount > 0) && (pdfLista[0].Miniatura != null))
            {
                // Width
                dataGridView1.Columns[0].Width = 100;

                //Height
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    if (pdfLista[i].Miniatura.Height > 100) { dataGridView1.Rows[i].Height = 100; }
                    else { dataGridView1.Rows[i].Height = pdfLista[i].Miniatura.Height;  }
                }
                dataGridView1.Refresh();
            }
        }
        private void setProp(bool estado)
        {
            propAuthor.Enabled = estado;
            propTitle.Enabled = estado;
            propCategory.Enabled = estado;
        }
        private void resetProp()
        {
            propFileName.Text = string.Empty;
            propAuthor.Text = string.Empty;
            propTitle.Text = string.Empty;
            propPreview.Image = null;

            propAuthor.BackColor = Color.White;
            propTitle.BackColor = Color.White;
            propCategory.BackColor = Color.White;

            if (dataGridView1.Rows.Count == 0)
            {
                propCategory.Text = string.Empty;
            }

        }
        private void selecionarPDF()
        {
            if (dataGridView1.SelectedRows.Count >= 0)
            {
                Arquivo x = PdfSelecionado();
                propFileName.Text = x.Arquivo_Nome;
                propTitle.Text = x.Titulo;
                propAuthor.Text = x.Autor;

                propAuthor.BackColor = Color.White;
                propTitle.BackColor = Color.White;
                propCategory.BackColor = Color.White;

                propBtApply.Enabled = false;

                //Categoria atual
                string categoriaAtual = obterCategoria();
                propCategory.SelectedIndex = propCategory.FindStringExact(categoriaAtual);

                string thumbBig = thumbDir + Path.GetFileNameWithoutExtension(x.Arquivo_Nome) + "_big.png";
                if (File.Exists(thumbBig))
                {
                    Miniatura = Image.FromFile(thumbBig);
                    propPreview.Image = Miniatura;
                } else
                {
                    propPreview.Image = null;
                }

                setProp(true);
            }
        }
        private void propAplicar()
        {
            int index = dataGridView1.CurrentRow.Index;

            //Atualiza Autor
            if (pdfLista[index].Autor != propAuthor.Text)
            {
                pdfLista[index].Autor = propAuthor.Text;
                btSave.Enabled = true;
                propAuthor.BackColor = Color.White;
                pdfLista[index].modificar = true;
            }

            //Atualiza Título
            if (pdfLista[index].Titulo != propTitle.Text)
            {
                pdfLista[index].Titulo = propTitle.Text;
                btSave.Enabled = true;
                propTitle.BackColor = Color.White;
                pdfLista[index].modificar = true;
            }

            //Atualiza categoria (e muda de diretório)
            string categoriaAtual = obterCategoria();
            string novaCategoria = propCategory.Text;
            if (categoriaAtual != novaCategoria)
            {
                string atualFullPath = pdfLista[index].Arquivo_FullPath;
                string newFullPath = textboxDir.Text + novaCategoria + @"\" + pdfLista[index].Arquivo_Nome;

                newFullPath = newFullPath.Replace(@"\\", @"\");

                if (!Directory.Exists(Path.GetDirectoryName(newFullPath)))
                {
                    try { Directory.CreateDirectory(Path.GetDirectoryName(newFullPath)); }
                    catch { userInfoMsg("!Error creating new directory"); }
                }
                if (Directory.Exists(Path.GetDirectoryName(newFullPath)))
                {
                    try { File.Move(atualFullPath, newFullPath); }
                    catch { }
                }
                if (File.Exists(newFullPath))
                {
                    pdfLista[dataGridView1.CurrentRow.Index].Arquivo_FullPath = newFullPath;
                    carregarCategorias();
                    propCategory.Text = novaCategoria;
                    propCategory.BackColor = Color.White;
                    dataGridView1.Refresh();
                }
                else
                {
                    propCategory.Text = obterCategoria();
                    userInfoMsg("!Error changing category");
                }
            }


            propBtApply.Enabled = false;
            dataGridView1.Refresh();
        }
        string obterCategoria()
        {
            string categoria = string.Empty;
            if (dataGridView1.SelectedRows.Count >= 0)
            {
                Arquivo x = PdfSelecionado();
                categoria = Path.GetDirectoryName(x.Arquivo_FullPath);
                categoria = categoria.Replace(textboxDir.Text, "");
            }
            if (string.IsNullOrEmpty(categoria)) { categoria = @"\"; }
            return categoria;
        }

        Arquivo PdfSelecionado()
        {
            return pdfLista[dataGridView1.CurrentRow.Index];
        }
        private void limparCache()
        {
            if (Directory.Exists(thumbDir))
            {
                // Apagar cache de Thumbs
                string[] picList = Directory.GetFiles(thumbDir, "*.png");
                foreach (string f in picList)
                {
                    try { File.Delete(f); }
                    catch { }
                }
            }
        }
        private void carregarCategorias()
        {
            string[] categorias = Directory.GetDirectories(textboxDir.Text, "**", SearchOption.AllDirectories);

            propCategory.Items.Clear();
            dropdownCategorias.DropDownItems.Clear();

            propCategory.Items.Add(@"\");
            dropdownCategorias.DropDownItems.Add(@"\");

            foreach (string categoria in categorias)
            {
                propCategory.Items.Add(categoria.Replace(textboxDir.Text, ""));
                dropdownCategorias.DropDownItems.Add(categoria.Replace(textboxDir.Text, ""));
            }

            toolStrip4.Update();
        }
        string pastaAtual()
        {
            string pasta = string.Empty;
            if (dropdownCategorias.Text == @"\")  { pasta = textboxDir.Text; }
            else { pasta = textboxDir.Text + dropdownCategorias.Text; }
            return pasta;
        }
        bool IsPasswordProtected(string pdfFullname)
        {
            try
            {
                PdfDocument document = PdfReader.Open(pdfFullname);
                return false;
            }
            catch (PdfReaderException)
            {
                return true;
            }
        }
        private void corrigeCategoria()
        {
            if (!string.IsNullOrEmpty(propCategory.Text))
            {
                //Coloca barra invertida no início se não houver
                if (propCategory.Text.Substring(0, 1) != @"\") { propCategory.Text = @"\" + propCategory.Text; }
                // Remover barra invertida no final se houver
                if (propCategory.Text.Substring(propCategory.Text.Length - 1, 1) == @"\") { propCategory.Text = propCategory.Text.Remove(propCategory.Text.Length - 1); }
            }
            else
            {
                propCategory.Text = @"\";
            }
        }
        private void marcarDelete()
        {
            int index = dataGridView1.CurrentRow.Index;

            if (pdfLista[index].deletar == true)
            {
                pdfLista[index].deletar = false;
                dataGridView1.Rows[index].DefaultCellStyle.ForeColor = Color.Black;
            }
            else
            {
                pdfLista[index].deletar = true;
                dataGridView1.Rows[index].DefaultCellStyle.ForeColor = Color.Red;
            }
            btSave.Enabled = true;
        }
        private void clearAllNewFN()
        {
            foreach (Arquivo x in pdfLista)
            {
                x.Arquivo_Novo_Nome = String.Empty;    
            }
            dataGridView1.Refresh();
        }
     }
}
