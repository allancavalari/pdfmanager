using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;


namespace PDFman
{

    public partial class PDFManager : Form
    {

        public PDFManager()
        {
            InitializeComponent();
        }
        private void toolStripButton6_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("PDFmanager 0.1b\n\n"+@"https://sites.google.com/view/pdfmanager"+"\n");
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            reajustarGrid();
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0) { salvar(); }
        }

        private void btLoad_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textboxDir.Text))
            {
                folderBrowserDialog1.SelectedPath = textboxDir.Text;
            }

            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (!String.IsNullOrEmpty(folderBrowserDialog1.SelectedPath.ToString()))
                {
                    textboxDir.Text = folderBrowserDialog1.SelectedPath.ToString();
                    if (Directory.Exists(textboxDir.Text))
                    {
                        userInfoMsg("Loading...");
                        carregar(textboxDir.Text);
                    }
                    else
                    {
                        userInfoMsg("!Error: Directory does not exist");
                    }
                }
            }

        }

        private void btOpenFolder_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(pastaAtual()))
            {
                System.Diagnostics.Process.Start(pastaAtual());
            }
            else
            {
                MessageBox.Show("Diretório inexistente!");
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow.Index >= 0)
            {
                selecionarPDF();
            }
        }

        private void textboxDir_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                // Remover barra no final se houver
                if (textboxDir.Text.Substring(textboxDir.Text.Length - 1, 1) == @"\")
                {
                    textboxDir.Text = textboxDir.Text.Remove(textboxDir.Text.Length - 1);
                }

                if (Directory.Exists(textboxDir.Text))
                {
                    userInfoMsg("Loading...");
                    carregar(textboxDir.Text);
                }
            }
        }

        private void propRunPdf_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow.Index >= 0)
            {
                Arquivo x = pdfLista[dataGridView1.CurrentRow.Index];
                System.Diagnostics.Process.Start(x.Arquivo_FullPath);
            }
        }

        private void propApply_Click(object sender, EventArgs e)
        {
            corrigeCategoria();
            propAplicar();
            propBtApply.Enabled = false;
        }

        private void propTitle_KeyPress(object sender, KeyPressEventArgs e)
        {
            propBtApply.Enabled = true;
            propTitle.BackColor = Color.Yellow;
            if (e.KeyChar == (char)13)
            {
                propAplicar();
            }
        }

        private void propAuthor_KeyPress(object sender, KeyPressEventArgs e)
        {
            propBtApply.Enabled = true;
            propAuthor.BackColor = Color.Yellow;
            if (e.KeyChar == (char)13)
            {
                propAplicar();
            }
        }

        private void propBtDeletePdf_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow.Index >= 0)
            {
                marcarDelete();
            }          
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(keepCache))
            {
                string limparCacheState = System.IO.File.ReadAllText(keepCache);
                if (limparCacheState == "False") {
                    limparCache();
                }
                else {
                    btKeepCache.Checked = true;
                }
            }
            if (File.Exists(pathFile)) {
                textboxDir.Text = System.IO.File.ReadAllText(pathFile);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btSave.Enabled == true)
            {
                DialogResult dr = MessageBox.Show("The changes was not saved. Are you sure you want to exit? ", "Exit confirmation", MessageBoxButtons.YesNo);
                if (dr == DialogResult.No) e.Cancel = true;
            }
        }

        private void btRefresh_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(pastaAtual()) && Directory.Exists(pastaAtual()))
            {
                userInfoMsg("Loading...");
                if (btSave.Enabled == true)
                {
                    DialogResult dr = MessageBox.Show("The changes was not saved. Are you sure you want refresh? ", "Confirmation", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes) { carregar(pastaAtual()); }
                }
                else
                {
                    carregar(pastaAtual());
                }               
            }
        }

        private void propCategory_KeyPress(object sender, KeyPressEventArgs e)
        {
            propBtApply.Enabled = true;
            propCategory.BackColor = Color.Yellow;

            if (e.KeyChar == (char)13)
            {
                corrigeCategoria();
                propAplicar();
            }
        }

        private void propCategory_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string categoriaAtual = obterCategoria();
                string novaCategoria = propCategory.Items[propCategory.SelectedIndex].ToString();

                if (categoriaAtual != novaCategoria)
                {
                    propBtApply.Enabled = true;
                    propCategory.BackColor = Color.Yellow;
                }
            }
        }

        private void dropdownCategorias_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string itemClicado = e.ClickedItem.Text;
            dropdownCategorias.DropDown.Close();

            Application.DoEvents();

            string pastaAbrir = string.Empty;
            if (itemClicado == @"\") { pastaAbrir = textboxDir.Text; }
            else { pastaAbrir = textboxDir.Text + itemClicado;  }

            if (!string.IsNullOrEmpty(textboxDir.Text) && Directory.Exists(textboxDir.Text))
            {
                userInfoMsg("Loading...");
                dropdownCategorias.Text = itemClicado;
                carregar(pastaAbrir);
            }
        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    marcarDelete();
                    e.Handled = true;
                }
            }
        }

        private void authorTitlepdfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0) { gerNomes(1); }
        }

        private void titleAuthorpdfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0) { gerNomes(2); }
        }

        private void titleAuthorpdfToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0) { gerNomes(3); }
        }

        private void titlepdfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0) { gerNomes(4); }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                System.Diagnostics.Process.Start(pdfLista[dataGridView1.CurrentRow.Index].Arquivo_FullPath);
            }
        }

        private void propPreview_Click_1(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string thumbBigPath = thumbDir + Path.GetFileNameWithoutExtension(pdfLista[dataGridView1.CurrentRow.Index].Arquivo_Nome) + "_big.png";
                if (File.Exists(thumbBigPath))
                { 
                    System.Diagnostics.Process.Start(thumbBigPath); 
                }
                
            }
        }

        private void btKeepCache_Click(object sender, EventArgs e)
        {
            System.IO.File.WriteAllText(keepCache, btKeepCache.Checked.ToString());
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clearAllNewFN();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if ((dataGridView1.SelectedRows.Count > 0)) { gerNomeIndividual(1); }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if ((dataGridView1.SelectedRows.Count > 0)) { gerNomeIndividual(2); }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if ((dataGridView1.SelectedRows.Count > 0)) { gerNomeIndividual(3); }
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if ((dataGridView1.SelectedRows.Count > 0)) { gerNomeIndividual(4); }
        }

        private void deleteNewFileNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((dataGridView1.SelectedRows.Count > 0)) { 
                PdfSelecionado().Arquivo_Novo_Nome = string.Empty;
                dataGridView1.Refresh();
            }
        }

        private void btLogMaximize_Click(object sender, EventArgs e)
        {
            splitContainer1.SplitterDistance = splitContainer1.Height - 100;
            btLogMaximize.Enabled = false;
            btLogMinimize.Enabled = true;
        }

        private void btLogMinimize_Click(object sender, EventArgs e)
        {
            splitContainer1.SplitterDistance = splitContainer1.Height - 26;
            btLogMaximize.Enabled = true;
            btLogMinimize.Enabled = false;
        }

        private void openActualPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(pastaAtual()))
            {
                System.Diagnostics.Process.Start(pastaAtual());
            }
            else
            {
                MessageBox.Show("Diretório inexistente!");
            }
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow.Index >= 0)
            {
                Arquivo x = pdfLista[dataGridView1.CurrentRow.Index];
                System.Diagnostics.Process.Start(x.Arquivo_FullPath);
            }
        }

        private void deleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow.Index >= 0)
            {
                marcarDelete();
            }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(pastaAtual()) && Directory.Exists(pastaAtual()))
            {
                userInfoMsg("Loading...");
                if (btSave.Enabled == true)
                {
                    DialogResult dr = MessageBox.Show("The changes was not saved. Are you sure you want refresh? ", "Confirmation", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes) { carregar(pastaAtual()); }
                }
                else
                {
                    carregar(pastaAtual());
                }
            }
        }

        private void copyFilePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow.Index >= 0)
            {
                Arquivo x = pdfLista[dataGridView1.CurrentRow.Index];
                System.Windows.Forms.Clipboard.SetText(x.Arquivo_FullPath);
            }
        }

        private void PDFManager_Shown(object sender, EventArgs e)
        {
            if (Directory.Exists(textboxDir.Text))
            {
                userInfoMsg("Loading...");
                carregar(textboxDir.Text);
            }
        }
    }
}
