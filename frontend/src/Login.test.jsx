import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import Login from './Login';
import { startDemo } from './api';

// Ersätt API-anropen med mockar – testerna ska inte nå någon server
vi.mock('./api', () => ({
  default: { post: vi.fn() },
  startDemo: vi.fn(),
}));

// Login använder <Link>, som kräver en router runt sig
const renderLogin = (onLoginSuccess = vi.fn()) =>
  render(
    <MemoryRouter>
      <Login onLoginSuccess={onLoginSuccess} />
    </MemoryRouter>,
  );

describe('Login – demo', () => {
  afterEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  it('visar demo-knapparna för chef och anställd', () => {
    // Arrange + Act
    renderLogin();

    // Assert
    expect(screen.getByRole('button', { name: 'Testa som chef' })).toBeTruthy();
    expect(
      screen.getByRole('button', { name: 'Testa som anställd' }),
    ).toBeTruthy();
  });

  it('startar demon som chef, sparar token och loggar in', async () => {
    // Arrange
    startDemo.mockResolvedValue('demo-token');
    const onLoginSuccess = vi.fn();
    renderLogin(onLoginSuccess);

    // Act
    fireEvent.click(screen.getByRole('button', { name: 'Testa som chef' }));

    // Assert
    await waitFor(() => expect(onLoginSuccess).toHaveBeenCalledTimes(1));
    expect(startDemo).toHaveBeenCalledWith('Manager');
    expect(localStorage.getItem('token')).toBe('demo-token');
  });

  it('visar API:ets felmeddelande när demon inte kan startas', async () => {
    // Arrange – t.ex. 429 när taket för sandlådor är nått
    startDemo.mockRejectedValue({
      response: {
        status: 429,
        data: { message: 'Demon är full just nu – försök igen om en stund.' },
      },
    });
    const onLoginSuccess = vi.fn();
    renderLogin(onLoginSuccess);

    // Act
    fireEvent.click(screen.getByRole('button', { name: 'Testa som anställd' }));

    // Assert
    expect(
      await screen.findByText(
        'Demon är full just nu – försök igen om en stund.',
      ),
    ).toBeTruthy();
    expect(onLoginSuccess).not.toHaveBeenCalled();
    expect(localStorage.getItem('token')).toBeNull();
  });
});
