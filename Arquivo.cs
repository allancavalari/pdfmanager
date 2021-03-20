using System;
using System.Drawing;
using System.ComponentModel;

namespace PDFman
{
    class Arquivo
    {
        public Arquivo()
        { }

        public Arquivo(Image _thumb, string _arquivo, string _arquivoNovoNome, string _titulo, string _autor, string _fullpath, bool _readonly, bool _deletar, bool _modificar)
        {
            this._thumb = _thumb;
            this._arquivo = _arquivo;
            this._titulo = _titulo;
            this._autor = _autor;
            this._arquivoFullPath = _fullpath;
            this._arquivoNovoNome = _arquivoNovoNome;
            this._readonly = _readonly;
            this._deletar = _deletar;
            this._modificar = _modificar;
        }

        private Image _thumb = null;
        private string _arquivo = String.Empty;
        private string _autor = String.Empty;
        private string _titulo = String.Empty;
        private string _arquivoFullPath = String.Empty;
        private string _arquivoNovoNome = String.Empty;
        private bool _readonly = false;
        private bool _deletar = false;
        private bool _modificar = false;

        [DisplayName("Preview")]
        public Image Miniatura
        {
            get { return _thumb; }
            set { _thumb = value; }
        }

        [DisplayName("Author")]
        public string Autor
        {
            get { return _autor; }
            set { _autor = value; }
        }

        [DisplayName("Title")]
        public string Titulo
        {
            get { return _titulo; }
            set { _titulo = value; }
        }

        [DisplayName("Fullpath")]
        public string Arquivo_FullPath
        {
            get { return _arquivoFullPath; }
            set { _arquivoFullPath = value; }
        }

        [DisplayName("Filename")]
        public string Arquivo_Nome
        {
            get { return _arquivo; }
            set { _arquivo = value; }
        }

        [DisplayName("New filename")]
        public string Arquivo_Novo_Nome
        {
            get { return _arquivoNovoNome; }
            set { _arquivoNovoNome = value; }
        }

        public bool read_only
        {
            get { return _readonly; }
            set { _readonly = value; }
        }
        public bool deletar
        {
            get { return _deletar; }
            set { _deletar = value; }
        }
        public bool modificar
        {
            get { return _modificar; }
            set { _modificar = value; }
        }
    }
}
