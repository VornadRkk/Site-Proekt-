import { Component, type ErrorInfo, type ReactNode } from 'react'

interface Props { children: ReactNode }
interface State { error: Error | null }

export default class AppErrorBoundary extends Component<Props, State> {
  state: State = { error: null }

  static getDerivedStateFromError(error: Error): State {
    return { error }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Ошибка при отрисовке приложения:', error, info)
  }

  render() {
    if (this.state.error) {
      return (
        <main className="app-error" role="alert">
          <div>
            <span>Ошибка запуска</span>
            <h1>Страница не смогла загрузиться</h1>
            <p>{this.state.error.message}</p>
            <button type="button" className="btn btn-dark rounded-pill px-4" onClick={() => window.location.reload()}>
              Обновить страницу
            </button>
          </div>
        </main>
      )
    }

    return this.props.children
  }
}
