import { useEffect, useMemo, useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import './App.css'

type AuthResponse = { token: string; nome: string; email: string; perfil: string }
type View = 'overview' | 'tickets' | 'knowledge'
type Ticket = { id: number; titulo: string; descricao: string; prioridade: string; status: string; dataCriacao: string; nomeUsuario?: string; nomeTecnico?: string; nomeCategoria?: string }
type Category = { id: number; nome: string; descricao?: string }
type KnowledgeArticle = { id: number; category: string; title: string; summary: string; content: string; tags: string[] }

const knowledgeArticles: KnowledgeArticle[] = [
  { id: 1, category: 'Acesso', title: 'Não consigo acessar minha conta', summary: 'Veja os passos para recuperar o acesso e validar suas credenciais.', content: 'Confirme se o e-mail está correto e tente redefinir a senha. Se sua conta estiver bloqueada ou inativa, abra um chamado na categoria Conta de Usuário informando o horário e a mensagem exibida.', tags: ['senha', 'login', 'conta'] },
  { id: 2, category: 'Acesso', title: 'Como solicitar uma nova permissão', summary: 'Entenda quais informações enviar ao suporte para liberar um sistema.', content: 'Informe seu nome, sistema desejado, nível de acesso necessário e a justificativa. O responsável pela área poderá ser consultado antes da liberação.', tags: ['permissão', 'acesso', 'sistema'] },
  { id: 3, category: 'Infraestrutura', title: 'VPN não conecta', summary: 'Checklist rápido para problemas de conexão com a rede corporativa.', content: 'Verifique sua conexão com a internet, reinicie o cliente VPN e confirme se a data e hora do computador estão corretas. Persistindo o erro, registre o código apresentado e crie um chamado.', tags: ['vpn', 'rede', 'conexão'] },
  { id: 4, category: 'Infraestrutura', title: 'Computador lento ou travando', summary: 'Informações que ajudam o suporte a diagnosticar problemas de desempenho.', content: 'Feche aplicações que não estão sendo usadas, reinicie o computador e verifique se há atualizações pendentes. Ao abrir o chamado, informe quando o problema começou e quais aplicativos estavam em uso.', tags: ['computador', 'desempenho', 'hardware'] },
  { id: 5, category: 'Aplicativo', title: 'Como descrever um erro', summary: 'Um bom relato reduz o tempo necessário para encontrar a causa.', content: 'Descreva o passo a passo para reproduzir o erro, o resultado esperado, o resultado obtido e, se possível, anexe uma captura de tela. Não inclua senhas ou dados sigilosos.', tags: ['erro', 'aplicativo', 'chamado'] },
  { id: 6, category: 'Chamados', title: 'Acompanhe o status do atendimento', summary: 'Conheça os estados usados no fluxo de suporte.', content: 'Aberto significa que a solicitação foi registrada. Em atendimento indica análise do suporte. Aguardando usuário depende de uma resposta sua. Resolvido aguarda confirmação ou fechamento.', tags: ['status', 'atendimento', 'fluxo'] },
]

const api = async <T,>(path: string, options: RequestInit = {}): Promise<T> => {
  const token = localStorage.getItem('helpdesk_token')
  const response = await fetch(`/api${path}`, { ...options, headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}), ...options.headers } })
  if (!response.ok) { const body = await response.json().catch(() => null); throw new Error(body?.message ?? 'Não foi possível concluir a operação.') }
  return response.status === 204 ? (undefined as T) : response.json()
}

function App() {
  const [session, setSession] = useState<AuthResponse | null>(() => { const stored = localStorage.getItem('helpdesk_session'); return stored ? JSON.parse(stored) : null })
  const [view, setView] = useState<View>('overview')
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [selectedTicket, setSelectedTicket] = useState<Ticket | null>(null)
  const [statusFilter, setStatusFilter] = useState('Todos')
  const [ticketSearch, setTicketSearch] = useState('')
  const [isCreateOpen, setCreateOpen] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const loadDashboard = async () => {
    setLoading(true)
    setError('')
    try {
      const [ticketData, categoryData] = await Promise.all([api<Ticket[]>('/chamados'), api<Category[]>('/categorias')])
      setTickets(ticketData)
      setCategories(categoryData)
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Erro ao carregar dados.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { if (session) void loadDashboard() }, [session])

  const filteredTickets = useMemo(() => tickets.filter((ticket) => {
    const matchesStatus = statusFilter === 'Todos' || ticket.status === statusFilter
    const term = ticketSearch.trim().toLowerCase()
    return matchesStatus && (!term || `${ticket.titulo} ${ticket.descricao} ${ticket.nomeCategoria ?? ''}`.toLowerCase().includes(term))
  }), [statusFilter, ticketSearch, tickets])

  const logout = () => { localStorage.removeItem('helpdesk_token'); localStorage.removeItem('helpdesk_session'); setSession(null) }
  if (!session) return <Login onLogin={(auth) => { localStorage.setItem('helpdesk_token', auth.token); localStorage.setItem('helpdesk_session', JSON.stringify(auth)); setSession(auth) }} />

  const pageTitle = view === 'overview' ? 'Visão geral' : view === 'tickets' ? 'Meus chamados' : 'Base de conhecimento'
  const pageDescription = view === 'overview' ? 'Acompanhe a saúde do atendimento e resolva o que precisa de atenção.' : view === 'tickets' ? 'Consulte suas solicitações, acompanhe o andamento e responda quando necessário.' : 'Encontre orientações rápidas antes de abrir uma nova solicitação.'

  return <div className="app-shell">
    <aside className="sidebar">
      <div className="brand"><span className="brand-mark">HD</span><span>HelpDesk</span></div>
      <nav>
        <button className={`nav-item ${view === 'overview' ? 'active' : ''}`} onClick={() => setView('overview')}>▦ <span>Visão geral</span></button>
        <button className={`nav-item ${view === 'tickets' ? 'active' : ''}`} onClick={() => setView('tickets')}>◌ <span>Meus chamados</span></button>
        <button className={`nav-item ${view === 'knowledge' ? 'active' : ''}`} onClick={() => setView('knowledge')}>⌁ <span>Base de conhecimento</span></button>
      </nav>
      <div className="sidebar-footer"><span className="status-dot" /> API conectada<span className="version">v1.0</span></div>
    </aside>
    <main className="main-content">
      <header className="topbar"><div className="breadcrumb">Workspace <span>/</span> {pageTitle}</div><div className="profile"><div className="avatar">{session.nome.charAt(0)}</div><div><strong>{session.nome}</strong><small>{session.perfil}</small></div><button className="icon-button" onClick={logout} title="Sair">↪</button></div></header>
      <section className="page-heading"><div><p className="eyebrow">CENTRAL DE SUPORTE</p><h1>{pageTitle}</h1><p className="muted">{pageDescription}</p></div>{view !== 'knowledge' && <button className="primary-button" onClick={() => setCreateOpen(true)}>+ Novo chamado</button>}</section>
      {error && <div className="alert">{error}</div>}
      {view === 'knowledge' ? <KnowledgeBase /> : <TicketWorkspace view={view} tickets={tickets} filteredTickets={filteredTickets} loading={loading} statusFilter={statusFilter} setStatusFilter={setStatusFilter} ticketSearch={ticketSearch} setTicketSearch={setTicketSearch} onSelect={setSelectedTicket} />}
    </main>
    {isCreateOpen && <CreateTicket categories={categories} onClose={() => setCreateOpen(false)} onCreated={() => { setCreateOpen(false); void loadDashboard() }} />}
    {selectedTicket && <TicketDetails ticket={selectedTicket} onClose={() => setSelectedTicket(null)} onUpdated={() => { setSelectedTicket(null); void loadDashboard() }} />}
  </div>
}

function TicketWorkspace({ view, tickets, filteredTickets, loading, statusFilter, setStatusFilter, ticketSearch, setTicketSearch, onSelect }: { view: View; tickets: Ticket[]; filteredTickets: Ticket[]; loading: boolean; statusFilter: string; setStatusFilter: (value: string) => void; ticketSearch: string; setTicketSearch: (value: string) => void; onSelect: (ticket: Ticket) => void }) {
  return <>
    {view === 'overview' && <section className="stats-grid"><Stat label="Chamados totais" value={tickets.length} hint="no seu escopo" /><Stat label="Em atendimento" value={tickets.filter((ticket) => ticket.status === 'EmAtendimento').length} hint="em andamento" accent="amber" /><Stat label="Alta prioridade" value={tickets.filter((ticket) => ticket.prioridade === 'Alta' || ticket.prioridade === 'Critica').length} hint="requer atenção" accent="red" /><Stat label="Resolvidos" value={tickets.filter((ticket) => ticket.status === 'Resolvido' || ticket.status === 'Fechado').length} hint="finalizados" accent="green" /></section>}
    <section className="workspace-panel">
      <div className="panel-header"><div><h2>{view === 'overview' ? 'Chamados recentes' : 'Todas as minhas solicitações'}</h2><p className="muted">{view === 'overview' ? 'Acompanhe cada solicitação em um só lugar.' : `${tickets.length} chamado(s) no seu escopo`}</p></div><div className="filters">{['Todos', 'Aberto', 'EmAtendimento', 'Resolvido'].map((filter) => <button key={filter} className={statusFilter === filter ? 'filter active-filter' : 'filter'} onClick={() => setStatusFilter(filter)}>{filter === 'EmAtendimento' ? 'Em atendimento' : filter}</button>)}</div></div>
      <div className="ticket-tools"><label className="search-field">⌕<input value={ticketSearch} onChange={(event) => setTicketSearch(event.target.value)} placeholder="Buscar por título, descrição ou categoria" /></label><span>{filteredTickets.length} resultado(s)</span></div>
      <div className="ticket-list">{loading ? <div className="empty-state">Carregando chamados...</div> : filteredTickets.length === 0 ? <div className="empty-state">Nenhum chamado encontrado neste filtro.</div> : filteredTickets.map((ticket) => <button className="ticket-row" key={ticket.id} onClick={() => onSelect(ticket)}><div className={`priority-line ${ticket.prioridade.toLowerCase()}`} /><div className="ticket-main"><span className="ticket-id">#{String(ticket.id).padStart(4, '0')}</span><strong>{ticket.titulo}</strong><p>{ticket.descricao}</p></div><div className="ticket-category">{ticket.nomeCategoria ?? 'Sem categoria'}</div><span className={`status status-${ticket.status.toLowerCase()}`}>{ticket.status === 'EmAtendimento' ? 'Em atendimento' : ticket.status}</span><span className="chevron">›</span></button>)}</div>
    </section>
  </>
}

function KnowledgeBase() {
  const [search, setSearch] = useState('')
  const [selectedArticle, setSelectedArticle] = useState<number | null>(null)
  const articles = useMemo(() => knowledgeArticles.filter((article) => `${article.title} ${article.summary} ${article.category} ${article.tags.join(' ')}`.toLowerCase().includes(search.trim().toLowerCase())), [search])
  return <section className="knowledge-page"><label className="knowledge-search">⌕<input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Busque por assunto, palavra-chave ou categoria" /></label><div className="knowledge-grid">{articles.map((article) => <article className={`knowledge-card ${selectedArticle === article.id ? 'expanded' : ''}`} key={article.id}><span className="article-category">{article.category}</span><h2>{article.title}</h2><p>{article.summary}</p>{selectedArticle === article.id && <div className="article-content"><p>{article.content}</p><div className="article-tags">{article.tags.map((tag) => <span key={tag}>#{tag}</span>)}</div></div>}<button className="text-button" onClick={() => setSelectedArticle(selectedArticle === article.id ? null : article.id)}>{selectedArticle === article.id ? 'Fechar artigo' : 'Ler artigo'} <span>→</span></button></article>)}</div>{articles.length === 0 && <div className="empty-state knowledge-empty">Nenhum artigo encontrado. Tente outro termo de busca.</div>}</section>
}

function Login({ onLogin }: { onLogin: (auth: AuthResponse) => void }) { const [email, setEmail] = useState('admin@helpdesk.com'); const [password, setPassword] = useState('admin123'); const [error, setError] = useState(''); const submit = async (event: FormEvent) => { event.preventDefault(); setError(''); try { onLogin(await api<AuthResponse>('/auth/login', { method: 'POST', body: JSON.stringify({ email, senha: password }) })) } catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'Falha ao entrar.') } }; return <main className="login-page"><div className="login-visual"><div className="visual-grid" /><p className="eyebrow">SUPORTE SEM RUÍDO</p><h1>O trabalho importante merece um fluxo claro.</h1><p>Uma central de atendimento feita para transformar solicitações em resoluções.</p><div className="visual-note"><span>●</span> Operação em tempo real</div></div><div className="login-card"><div className="brand"><span className="brand-mark">HD</span><span>HelpDesk</span></div><div className="login-copy"><p className="eyebrow">BEM-VINDO DE VOLTA</p><h2>Acesse sua central</h2><p className="muted">Entre para acompanhar seus chamados.</p></div><form onSubmit={submit}><label>E-mail<input type="email" value={email} onChange={(event) => setEmail(event.target.value)} required /></label><label>Senha<input type="password" value={password} onChange={(event) => setPassword(event.target.value)} required /></label>{error && <div className="form-error">{error}</div>}<button className="primary-button full-button">Entrar na plataforma <span>→</span></button></form><small className="demo-hint">Demonstração: admin@helpdesk.com / admin123</small></div></main> }
function Stat({ label, value, hint, accent = 'blue' }: { label: string; value: number; hint: string; accent?: string }) { return <div className={`stat-card stat-${accent}`}><div className="stat-top"><span>{label}</span><span className="stat-spark">↗</span></div><strong>{value}</strong><small>{hint}</small></div> }
function CreateTicket({ categories, onClose, onCreated }: { categories: Category[]; onClose: () => void; onCreated: () => void }) { const [form, setForm] = useState({ titulo: '', descricao: '', prioridade: 'Media', categoriaId: categories[0]?.id ?? 0 }); const [error, setError] = useState(''); const [saving, setSaving] = useState(false); const submit = async (event: FormEvent) => { event.preventDefault(); setSaving(true); try { await api('/chamados', { method: 'POST', body: JSON.stringify(form) }); onCreated() } catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'Erro ao criar chamado.') } finally { setSaving(false) } }; return <Modal title="Novo chamado" onClose={onClose}><form className="modal-form" onSubmit={submit}><label>Título<input value={form.titulo} onChange={(event) => setForm({ ...form, titulo: event.target.value })} placeholder="Ex.: Acesso ao sistema indisponível" required /></label><label>Descrição<textarea value={form.descricao} onChange={(event) => setForm({ ...form, descricao: event.target.value })} placeholder="Descreva o que aconteceu..." rows={4} required /></label><div className="form-columns"><label>Prioridade<select value={form.prioridade} onChange={(event) => setForm({ ...form, prioridade: event.target.value })}>{['Baixa', 'Media', 'Alta', 'Critica'].map((item) => <option key={item}>{item}</option>)}</select></label><label>Categoria<select value={form.categoriaId} onChange={(event) => setForm({ ...form, categoriaId: Number(event.target.value) })}>{categories.map((category) => <option key={category.id} value={category.id}>{category.nome}</option>)}</select></label></div>{error && <div className="form-error">{error}</div>}<div className="modal-actions"><button type="button" className="secondary-button" onClick={onClose}>Cancelar</button><button className="primary-button" disabled={saving}>{saving ? 'Salvando...' : 'Criar chamado'}</button></div></form></Modal> }
function TicketDetails({ ticket, onClose, onUpdated }: { ticket: Ticket; onClose: () => void; onUpdated: () => void }) { const [status, setStatus] = useState(ticket.status); const [error, setError] = useState(''); const update = async () => { try { await api(`/chamados/${ticket.id}`, { method: 'PUT', body: JSON.stringify({ status }) }); onUpdated() } catch (requestError) { setError(requestError instanceof Error ? requestError.message : 'Erro ao atualizar chamado.') } }; return <Modal title={`Chamado #${String(ticket.id).padStart(4, '0')}`} onClose={onClose}><div className="detail-title"><span className={`priority-badge ${ticket.prioridade.toLowerCase()}`}>{ticket.prioridade}</span><h2>{ticket.titulo}</h2></div><p className="detail-description">{ticket.descricao}</p><div className="detail-meta"><span><small>Categoria</small>{ticket.nomeCategoria ?? 'Não definida'}</span><span><small>Solicitante</small>{ticket.nomeUsuario ?? 'Você'}</span><span><small>Criado em</small>{new Date(ticket.dataCriacao).toLocaleDateString('pt-BR')}</span></div><label>Status<select value={status} onChange={(event) => setStatus(event.target.value)}>{['Aberto', 'EmAtendimento', 'AguardandoUsuario', 'Resolvido', 'Fechado'].map((item) => <option key={item}>{item}</option>)}</select></label>{error && <div className="form-error">{error}</div>}<div className="modal-actions"><button className="secondary-button" onClick={onClose}>Fechar</button><button className="primary-button" onClick={update}>Atualizar status</button></div></Modal> }
function Modal({ title, onClose, children }: { title: string; onClose: () => void; children: ReactNode }) { return <div className="modal-backdrop" onMouseDown={(event) => event.target === event.currentTarget && onClose()}><div className="modal"><div className="modal-header"><h2>{title}</h2><button className="close-button" onClick={onClose} title="Fechar">×</button></div>{children}</div></div> }

export default App
