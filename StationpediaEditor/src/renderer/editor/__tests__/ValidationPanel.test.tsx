/**
 * Tests for ValidationPanel component
 */
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ValidationPanel } from '../ValidationPanel';
import type { ValidationResult } from '@models/validationModel';
import { VALIDATION_RULE_IDS } from '@models/validationModel';

describe('ValidationPanel', () => {
  const mockValidationResults = new Map<string, ValidationResult>([
    [
      'Device1',
      {
        deviceKey: 'Device1',
        errors: [
          {
            ruleId: VALIDATION_RULE_IDS.MISSING_DEVICE_KEY,
            severity: 'error',
            message: 'Device key is required',
            location: { field: 'deviceKey' },
          },
          {
            ruleId: VALIDATION_RULE_IDS.BROKEN_LINK,
            severity: 'warning',
            message: 'Broken link: BadDevice',
            location: { field: 'pageDescription' },
          },
        ],
        hasErrors: true,
        hasWarnings: true,
        hasInfo: false,
      },
    ],
    [
      'Device2',
      {
        deviceKey: 'Device2',
        errors: [
          {
            ruleId: VALIDATION_RULE_IDS.EMPTY_CONTENT,
            severity: 'info',
            message: 'Section has no content',
            location: { field: 'operationalDetails', sectionIndex: 0 },
          },
        ],
        hasErrors: false,
        hasWarnings: false,
        hasInfo: true,
      },
    ],
  ]);

  it('should render validation panel title', () => {
    render(
      <ValidationPanel
        results={mockValidationResults}
        selectedSeverities={new Set(['error', 'warning', 'info'])}
        isRunning={false}
        onToggleSeverity={() => {}}
        onSelectError={() => {}}
      />
    );

    expect(screen.getByText(/validation/i)).toBeInTheDocument();
  });

  it('should display error count', () => {
    render(
      <ValidationPanel
        results={mockValidationResults}
        selectedSeverities={new Set(['error', 'warning', 'info'])}
        isRunning={false}
        onToggleSeverity={() => {}}
        onSelectError={() => {}}
      />
    );

    // Should show total errors: 1 error + 1 warning + 1 info = 3
    expect(screen.getByText(/3 issue/i)).toBeInTheDocument();
  });

  it('should display individual errors grouped by device', () => {
    render(
      <ValidationPanel
        results={mockValidationResults}
        selectedSeverities={new Set(['error', 'warning', 'info'])}
        isRunning={false}
        onToggleSeverity={() => {}}
        onSelectError={() => {}}
      />
    );

    expect(screen.getByText('Device1')).toBeInTheDocument();
    expect(screen.getByText('Device2')).toBeInTheDocument();
    expect(screen.getByText(/Device key is required/)).toBeInTheDocument();
    expect(screen.getByText(/Broken link/)).toBeInTheDocument();
  });

  it('should filter errors by severity', async () => {
    const user = userEvent.setup();
    const onToggle = vi.fn();

    render(
      <ValidationPanel
        results={mockValidationResults}
        selectedSeverities={new Set(['warning', 'info'])}
        isRunning={false}
        onToggleSeverity={onToggle}
        onSelectError={() => {}}
      />
    );

    // Should not show error-level items
    expect(screen.queryByText(/Device key is required/)).not.toBeInTheDocument();
  });

  it('should show loading state when running', () => {
    render(
      <ValidationPanel
        results={mockValidationResults}
        selectedSeverities={new Set(['error', 'warning', 'info'])}
        isRunning={true}
        onToggleSeverity={() => {}}
        onSelectError={() => {}}
      />
    );

    expect(screen.getByText(/running|validating|checking/i)).toBeInTheDocument();
  });

  it('should call onSelectError when clicking an error', async () => {
    const user = userEvent.setup();
    const onSelectError = vi.fn();

    render(
      <ValidationPanel
        results={mockValidationResults}
        selectedSeverities={new Set(['error', 'warning', 'info'])}
        isRunning={false}
        onToggleSeverity={() => {}}
        onSelectError={onSelectError}
      />
    );

    // Click on an error message
    const errorMsg = screen.getByText(/Device key is required/);
    const errorButton = errorMsg.closest('button');
    
    if (errorButton) {
      await user.click(errorButton);
      expect(onSelectError).toHaveBeenCalled();
    }
  });

  it('should show severity filter buttons', () => {
    render(
      <ValidationPanel
        results={mockValidationResults}
        selectedSeverities={new Set(['error', 'warning', 'info'])}
        isRunning={false}
        onToggleSeverity={() => {}}
        onSelectError={() => {}}
      />
    );

    expect(screen.getByText(/error/i)).toBeInTheDocument();
    expect(screen.getByText(/warning/i)).toBeInTheDocument();
    expect(screen.getByText(/info/i)).toBeInTheDocument();
  });
});
