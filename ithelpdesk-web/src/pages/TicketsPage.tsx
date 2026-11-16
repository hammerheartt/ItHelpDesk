import { useAuth } from '../auth/AuthContext'

export default function TicketsPage() {
  const { user } = useAuth()
  const title =
    user!.role === 'Employee' ? 'My tickets' :
    user!.role === 'Technician' ? 'Assigned to me' :
    'All tickets'

  return (
    <>
      <h1>{title}</h1>
      <div className="panel">Ticket list comes in the next step.</div>
    </>
  )
}