import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen, fireEvent, act } from '@testing-library/react';
import { ToastProvider, useToast } from './ToastContext';

// Testkomponent som visar en toast när knappen klickas
const Trigger = () => {
  const toast = useToast();
  return <button onClick={() => toast.success('Sparat!')}>Visa</button>;
};

describe('ToastProvider', () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it('visar en toast och tar bort den efter 4 sekunder', () => {
    // Arrange – falska timers så att testet inte behöver vänta på riktigt
    vi.useFakeTimers();
    render(
      <ToastProvider>
        <Trigger />
      </ToastProvider>,
    );

    // Act
    fireEvent.click(screen.getByText('Visa'));

    // Assert – toasten syns direkt
    expect(screen.getByText('Sparat!')).toBeTruthy();

    // Act – 4 s till auto-dismiss + 300 ms ut-animation
    act(() => {
      vi.advanceTimersByTime(4300);
    });

    // Assert – queryByText returnerar null istället för att kasta fel
    expect(screen.queryByText('Sparat!')).toBeNull();
  });

  it('ger samma toast-objekt mellan renderingar', () => {
    // Arrange – sparar toast-objektet från varje rendering
    const seen = [];
    const Capture = () => {
      seen.push(useToast());
      return null;
    };
    const { rerender } = render(
      <ToastProvider>
        <Capture />
      </ToastProvider>,
    );

    // Act
    rerender(
      <ToastProvider>
        <Capture />
      </ToastProvider>,
    );

    // Assert – samma objekt gör att toast kan ligga i en useEffects beroendelista
    // utan att effekten körs om vid varje rendering
    expect(seen.length).toBeGreaterThan(1);
    expect(seen[1]).toBe(seen[0]);
  });
});
